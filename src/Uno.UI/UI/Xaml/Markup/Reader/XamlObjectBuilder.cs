﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Uno.Extensions;
using Uno.UI;
using Uno.UI.Helpers.Xaml;
using Uno.UI.Xaml;
using Uno.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Text;
using Windows.Foundation.Metadata;
using Color = Windows.UI.Color;

#if XAMARIN_ANDROID
using _View = Android.Views.View;
#elif XAMARIN_IOS_UNIFIED
using _View = UIKit.UIView;
#else
using _View = Windows.UI.Xaml.UIElement;
#endif

namespace Windows.UI.Xaml.Markup.Reader
{
	internal class XamlObjectBuilder
	{
		private XamlFileDefinition _fileDefinition;
		private XamlTypeResolver TypeResolver { get; }
		private readonly List<(string elementName, ElementNameSubject bindingSubject)> _elementNames = new List<(string, ElementNameSubject)>();
		private readonly Stack<Type> _styleTargetTypeStack = new Stack<Type>();
		private Queue<Action> _postActions = new Queue<Action>();
		private static readonly Regex _attachedPropertMatch = new Regex(@"(\(.*?\))");

		private static Type[] _genericConvertibles = new[]
		{
			typeof(Media.Brush),
			typeof(Media.SolidColorBrush),
			typeof(Color),
			typeof(Thickness),
			typeof(CornerRadius),
			typeof(Media.FontFamily),
			typeof(GridLength),
			typeof(Media.Animation.KeyTime),
			typeof(Duration),
			typeof(Media.Matrix),
			typeof(FontWeight),
		};

		public XamlObjectBuilder(XamlFileDefinition xamlFileDefinition)
		{
			_fileDefinition = xamlFileDefinition;
			TypeResolver = new XamlTypeResolver(_fileDefinition);
		}

		internal object? Build(object? component = null, bool createInstanceFromXClass = false)
		{
			var topLevelControl = _fileDefinition.Objects.First();

			var instance = LoadObject(topLevelControl, rootInstance: null, component: component, createInstanceFromXClass: createInstanceFromXClass);

			ApplyPostActions(instance);

			return instance;
		}

		private object? LoadObject(XamlObjectDefinition? control, object? rootInstance, object? component = null, bool createInstanceFromXClass = false)
		{
			if (control == null)
			{
				return null;
			}

			if (
				control.Type.Name == "NullExtension"
				&& control.Type.PreferredXamlNamespace == XamlConstants.XamlXmlNamespace
			)
			{
				return null;
			}

			var type = TypeResolver.FindType(control.Type);
			var classMember = control.Members.FirstOrDefault(m => m.Member.Name == "Class" && m.Member.PreferredXamlNamespace == XamlConstants.XamlXmlNamespace);

			if (createInstanceFromXClass && TypeResolver.FindType(classMember?.Value?.ToString()) is { } classType)
			{
				return Activator.CreateInstance(classType);
			}

			if (type == null)
			{
				throw new InvalidOperationException($"Unable to find type {control.Type}");
			}

			var unknownContent = control.Members.Where(m => m.Member.Name == "_UnknownContent").FirstOrDefault();
			var unknownContentValue = unknownContent?.Value;
			var initializationMember = control.Members.Where(m => m.Member.Name == "_Initialization").FirstOrDefault();

			var isBrush = type == typeof(Media.Brush);

			if (type.Is<FrameworkTemplate>())
			{
				Func<_View?> builder = () =>
				{
					var contentOwner = unknownContent;

					return LoadObject(contentOwner?.Objects.FirstOrDefault(), rootInstance: rootInstance) as _View;
				};

				return Activator.CreateInstance(type, builder);
			}
			else if (type.Is<ResourceDictionary>() && unknownContent != null)
			{
				var contentOwner = unknownContent;

				if (Activator.CreateInstance(type) is ResourceDictionary rd)
				{
					foreach (var xamlObjectDefinition in contentOwner.Objects)
					{
						var key = xamlObjectDefinition.Members.FirstOrDefault(m => m.Member.Name == "Key")?.Value;

						var instance = LoadObject(xamlObjectDefinition, rootInstance: rootInstance);

						if (key != null)
						{
							rd.Add(key, instance);
						}
					}

					return rd;
				}
				else
				{
					throw new InvalidCastException();
				}
			}
			else if (type.IsPrimitive && initializationMember?.Value is string primitiveValue)
			{
				return Convert.ChangeType(primitiveValue, type, CultureInfo.InvariantCulture);
			}
			else if (type == typeof(string) && initializationMember?.Value is string stringValue)
			{
				return stringValue;
			}
			else if (
				_genericConvertibles.Contains(type)
				&& control.Members.Where(m => m.Member.Name == "_UnknownContent").FirstOrDefault()?.Value is string otherContentValue)
			{
				return XamlBindingHelper.ConvertValue(type, otherContentValue);
			}
			else
			{
				var instance = component ?? Activator.CreateInstance(type)!;
				rootInstance ??= instance;

				IDisposable? TryProcessStyle()
				{
					if (instance is Style style)
					{
						if (control.Members.FirstOrDefault(m => m.Member.Name == "TargetType") is XamlMemberDefinition targetTypeDefinition)
						{
							if (BuildLiteralValue(targetTypeDefinition) is Type targetType)
							{
								_styleTargetTypeStack.Push(targetType);

								return Uno.Disposables.Disposable.Create(
									() =>
									{
										if (_styleTargetTypeStack.Pop() != targetType)
										{
											throw new InvalidOperationException("StyleTargetType is out of synchronization");
										}
									}
								);
							}
							else
							{
								throw new InvalidOperationException($"The type {targetTypeDefinition.Member.Type} is unknown");
							}
						}
					}

					return null;
				}

				using (TryProcessStyle())
				{
					foreach (var member in control.Members)
					{
						ProcessNamedMember(control, instance, member, rootInstance);
					}
				}

				return instance;
			}
		}

		private string RewriteAttachedPropertyPath(string? value)
		{
			value ??= "";

			if (value.Contains("("))
			{
				foreach (var ns in _fileDefinition.Namespaces)
				{
					if (ns != null)
					{
						var clrNamespace = ns.Namespace.TrimStart("using:");

						value = value.Replace("(" + ns.Prefix + ":", "(" + clrNamespace + ":");
					}
				}

				var match = _attachedPropertMatch.Match(value);

				if (match.Success)
				{
					do
					{
						if (!match.Value.Contains(":"))
						{
							// if there is no ":" this means that the type is using the default
							// namespace, so try to resolve the best way we can.

							var parts = match.Value.Trim(new[] { '(', ')' }).Split(new[] { '.' });

							if (parts.Length == 2)
							{
								var targetType = TypeResolver.FindType(parts[0]);

								if (targetType != null)
								{
									var newPath = targetType.Namespace + ":" + targetType.Name + "." + parts[1];

									value = value.Replace(match.Value, '(' + newPath + ')');
								}
							}
						}
					}
					while ((match = match.NextMatch()).Success);
				}
			}

			return value;
		}

		private void ProcessNamedMember(
			XamlObjectDefinition control,
			object instance,
			XamlMemberDefinition member,
			object rootInstance)
		{
			// Exclude attached properties, must be set in the extended apply section.
			// If there is no type attached, this can be a binding.
			if (TypeResolver.IsType(control.Type, member.Member.DeclaringType)
				&& !TypeResolver.IsAttachedProperty(member)
				|| member.Member.Name == "_UnknownContent"
			// && FindEventType(member.Member) == null
			)
			{
				if (instance is TextBlock textBlock)
				{
					ProcessTextBlock(control, textBlock, member, rootInstance);
				}
				else if (instance is Documents.Span span && member.Member.Name == "_UnknownContent")
				{
					ProcessSpan(control, span, member, rootInstance);
				}
				else if (member.Member.Name == "_UnknownContent"
					&& TypeResolver.FindContentProperty(TypeResolver.FindType(control.Type)) == null
					&& TypeResolver.IsCollectionOrListType(TypeResolver.FindType(control.Type)))
				{
					AddCollectionItems(instance, member.Objects, rootInstance);
				}
				else if (GetMemberProperty(control, member) is PropertyInfo propertyInfo)
				{
					if (member.Objects.None())
					{
						if (TypeResolver.IsInitializedCollection(propertyInfo)
							// The Resources property has a public setter, but should be treated as an empty collection
							|| IsResourcesProperty(propertyInfo)
						)
						{
							// Empty collection
						}
						else
						{
							if (propertyInfo.PropertyType == typeof(TargetPropertyPath))
							{
								ProcessTargetPropertyPath(instance, member, propertyInfo);
							}
							else
							{
								GetPropertySetter(propertyInfo).Invoke(instance, new[] { BuildLiteralValue(member, propertyInfo.PropertyType) });
							}
						}
					}
					else
					{
						if (IsMarkupExtension(member))
						{
							ProcessMemberMarkupExtension(instance, rootInstance, member, propertyInfo);
						}
						else
						{
							ProcessMemberElements(instance, member, propertyInfo, rootInstance);
						}
					}
				}
				else if (GetMemberEvent(control, member) is EventInfo eventInfo)
				{
					if (member.Value is string eventHandlerName)
					{
						SubscribeToEvent(instance, rootInstance, eventInfo, eventHandlerName, false);
					}
					else if (member.Objects.FirstOrDefault() is { } memberObject && memberObject.Type.Name == "Bind")
					{
						if (memberObject.Members.FirstOrDefault() is { } bindMember && bindMember.Value is string xBindEventHandlerName)
						{
							SubscribeToEvent(instance, rootInstance, eventInfo, xBindEventHandlerName, true);
						}
					}
				}
				else
				{
					// throw new InvalidOperationException($"The Property {member.Member.Name} does not exist on {member.Member.DeclaringType}");
				}
			}
			else if (TypeResolver.IsAttachedProperty(member))
			{
				var dependencyProperty = TypeResolver.FindDependencyProperty(member);

				if (dependencyProperty != null)
				{
					if (member.Objects.None())
					{
						(instance as DependencyObject)?.SetValue(dependencyProperty, BuildLiteralValue(member, dependencyProperty.Type));
					}
					else
					{
						if (IsMarkupExtension(member))
						{
							ProcessMemberMarkupExtension(instance, rootInstance, member, null);
						}
						else if (instance is DependencyObject dependencyObject)
						{
							ProcessMemberElements(dependencyObject, member, dependencyProperty, rootInstance);
						}
						else
						{
							throw new InvalidOperationException($"{instance} is not a DependencyObject");
						}
					}
				}
			}
			else if (member.Member.DeclaringType == null && member.Member.Name == "Name")
			{
				// This is a special case, where the declaring type is from the x: namespace,
				// but is considered of an unknown type. This can happen when providing the
				// name of a control using x:Name instead of Name.
				if (TypeResolver.GetPropertyByName(control.Type, "Name") is PropertyInfo nameInfo)
				{
					GetPropertySetter(nameInfo).Invoke(instance, new[] { member.Value });
				}

				// Update x:Name generated fields, if any
				if (rootInstance != null && member.Value is string nameValue)
				{
					var allMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					var rootInstanceType = rootInstance.GetType();

					if (TypeResolver.GetPropertyByName(rootInstanceType, nameValue, allMembers) is { } xNameProperty)
					{
						GetPropertySetter(xNameProperty).Invoke(rootInstance, new[] { instance });
					}
					else if (TypeResolver.GetFieldByName(rootInstanceType, nameValue, allMembers) is { } xNameField)
					{
						xNameField.SetValue(rootInstance, instance);
					}
				}
			}
		}

		private static void SubscribeToEvent(object instance, object rootInstance, EventInfo eventInfo, string eventHandlerName, bool supportsParameterless)
		{
			var eventParamCount = eventInfo.EventHandlerType?.GetMethod("Invoke")?.GetParameters().Length ?? throw new InvalidOperationException();
			var rootType = rootInstance.GetType();
			var targetMethod = GetMethod(eventHandlerName, eventParamCount, rootType);
			if (targetMethod != null)
			{
				var handler = targetMethod.CreateDelegate(eventInfo.EventHandlerType, rootInstance);
				eventInfo.AddEventHandler(instance, handler);
			}
			else if (supportsParameterless && GetMethod(eventHandlerName, 0, rootType) is { } parameterlessMethod && eventParamCount <= 2)
			{
				var wrapper = new EventHandlerWrapper(rootInstance, parameterlessMethod);
				var wrappedHandler = (typeof(EventHandlerWrapper).GetMethod(eventParamCount == 2 ? nameof(EventHandlerWrapper.Handler2) : nameof(EventHandlerWrapper.Handler1)))
					?? throw new InvalidOperationException();
				var handler = Delegate.CreateDelegate(
					eventInfo.EventHandlerType,
					wrapper,
					wrappedHandler
				); ;
				eventInfo.AddEventHandler(instance, handler);
			}
		}

		private static MethodInfo? GetMethod(string methodName, int paramCount, Type type)
			=> type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(m => m.Name == methodName && m.GetParameters().Length == paramCount)
			.FirstOrDefault();

		private void ProcessSpan(XamlObjectDefinition control, Span span, XamlMemberDefinition member, object rootInstance)
		{
			if (member.Objects.Count != 0)
			{
				foreach (var node in member.Objects)
				{
					if (LoadObject(node, rootInstance) is Inline inline)
					{
						span.Inlines.Add(inline);
					}
				}
			}

			if (member.Value != null)
			{
				span.Inlines.Add(
					new Run
					{
						Text = member.Value.ToString()
					}
				);
			}
		}

		private void ProcessTargetPropertyPath(object instance, XamlMemberDefinition member, PropertyInfo propertyInfo)
		{
			if (member.Value is string targetPath)
			{
				// This builds property setters for specified member setter.
				var separatorIndex = targetPath.IndexOf(".");
				var elementName = targetPath.Substring(0, separatorIndex);
				var propertyName = targetPath.Substring(separatorIndex + 1);

				if (instance is Setter setter)
				{
					setter.Target = new TargetPropertyPath(elementName, new PropertyPath(RewriteAttachedPropertyPath(propertyName)));
				}
			}
			else
			{
				throw new NotSupportedException($"The property {propertyInfo} must be provided a value");
			}
		}

		private void ProcessTextBlock(XamlObjectDefinition control, TextBlock instance, XamlMemberDefinition member, object rootInstance)
		{
			if (member.Objects.Any())
			{
				if (IsMarkupExtension(member))
				{
					ProcessMemberMarkupExtension(instance, rootInstance, member, null);
				}
				else
				{
					foreach (var node in member.Objects)
					{
						if (LoadObject(node, rootInstance) is Inline inline)
						{
							instance.Inlines.Add(inline);
						}
					}
				}
			}
			else if (GetMemberProperty(control, member) is PropertyInfo propertyInfo)
			{
				GetPropertySetter(propertyInfo).Invoke(instance, new[] { BuildLiteralValue(member, propertyInfo.PropertyType) });
			}
		}

		private void ProcessMemberElements(DependencyObject instance, XamlMemberDefinition member, DependencyProperty property, object rootInstance)
		{
			if (TypeResolver.IsCollectionOrListType(property.Type))
			{
				object BuildInstance()
				{
					if (property.Type.GetGenericTypeDefinition() == typeof(IList<>))
					{
						return Activator.CreateInstance(typeof(List<>)!.MakeGenericType(property.Type.GenericTypeArguments[0]))!;
					}
					else
					{
						return Activator.CreateInstance(property.Type)!;
					}
				}

				var collection = BuildInstance();

				AddCollectionItems(collection, member.Objects, rootInstance);

				instance.SetValue(property, collection);
			}
			else
			{
				instance.SetValue(property, LoadObject(member.Objects.First(), rootInstance: rootInstance));
			}
		}

		private void ProcessMemberElements(object instance, XamlMemberDefinition member, PropertyInfo propertyInfo, object rootInstance)
		{
			if (TypeResolver.IsCollectionOrListType(propertyInfo.PropertyType))
			{
				if (propertyInfo.PropertyType == typeof(ResourceDictionary))
				{
					var methods = propertyInfo.PropertyType.GetMethods();
					var addMethod = propertyInfo.PropertyType.GetMethod("Add", new[] { typeof(object), typeof(object) })
						?? throw new InvalidOperationException($"The property {propertyInfo} type does not provide an Add method (Line {member.LineNumber}:{member.LinePosition}");

					foreach (var child in member.Objects)
					{
						var item = LoadObject(child, rootInstance: rootInstance);

						var resourceKey = GetResourceKey(child);
						var resourceTargetType = GetResourceTargetType(child);

						if (
							item?.GetType() == typeof(Style)
							&& resourceTargetType == null
						)
						{
							throw new InvalidOperationException($"No target type was specified (Line {member.LineNumber}:{member.LinePosition}");
						}

						if(propertyInfo.GetMethod == null)
						{
							throw new InvalidOperationException($"The property {propertyInfo} does not provide a getter (Line {member.LineNumber}:{member.LinePosition}");
						}

						var propertyInstance = propertyInfo.GetMethod.Invoke(instance, null);

						addMethod.Invoke(propertyInstance, new[] { resourceKey ?? resourceTargetType, item });
					}
				}
				else if (propertyInfo.SetMethod?.IsPublic == true &&
					member.Objects.Select(x => x.Type).Distinct().All(x => TypeResolver.FindType(x).Is(propertyInfo.PropertyType)))
				{
					// It is actually valid to have multiple nested collection containers under a property, however only the last will be kept.
					foreach (var child in member.Objects)
					{
						var collection = LoadObject(child, rootInstance: rootInstance);

						GetPropertySetter(propertyInfo).Invoke(instance, new[] { collection });
					}
				}
				else if (propertyInfo.SetMethod?.IsPublic == true
					&& TypeResolver.IsNewableType(propertyInfo.PropertyType))
				{
					var collection = Activator.CreateInstance(propertyInfo.PropertyType);
					AddCollectionItems(collection!, member.Objects, rootInstance);

					GetPropertySetter(propertyInfo).Invoke(instance, new[] { collection });
				}
				else if (TypeResolver.IsInitializedCollection(propertyInfo))
				{
					if (propertyInfo.GetMethod == null)
					{
						throw new InvalidOperationException($"The property {propertyInfo} does not provide a getter (Line {member.LineNumber}:{member.LinePosition}");
					}

					var propertyInstance = propertyInfo.GetMethod.Invoke(instance, null);

					if (propertyInstance != null)
					{
						if (propertyInstance is IDictionary<object, object> propertyInstanceAsDictionary)
						{
							AddGenericDictionaryItems(propertyInstanceAsDictionary, member.Objects, rootInstance);
						}
						else
						{
							AddCollectionItems(propertyInstance, member.Objects, rootInstance);
						}
					}
					else
					{
						throw new InvalidOperationException($"The property {propertyInfo} getter did not provide a value (Line {member.LineNumber}:{member.LinePosition}");
					}
				}
				else
				{
					throw new InvalidOperationException($"Unsupported collection type {propertyInfo.PropertyType} on {propertyInfo}");
				}
			}
			else
			{
				GetPropertySetter(propertyInfo).Invoke(instance, new[] { LoadObject(member.Objects.First(), rootInstance: rootInstance) });
			}
		}

		private static MethodInfo GetPropertySetter(PropertyInfo propertyInfo)
			=> propertyInfo?.SetMethod ?? throw new InvalidOperationException($"Unable to find setter for property [{propertyInfo}]");

		private void ProcessMemberMarkupExtension(object instance, object? rootInstance, XamlMemberDefinition member, PropertyInfo? propertyInfo)
		{
			if (IsBindingMarkupNode(member))
			{
				ProcessBindingMarkupNode(instance, rootInstance, member);
			}
			else if (IsStaticResourceMarkupNode(member) || IsThemeResourceMarkupNode(member))
			{
				ProcessStaticResourceMarkupNode(instance, member, propertyInfo);
			}
		}

		private void ProcessStaticResourceMarkupNode(object instance, XamlMemberDefinition member, PropertyInfo? propertyInfo)
		{
			var resourceNode = member.Objects.FirstOrDefault();

			if (resourceNode != null)
			{
				var keyName = resourceNode.Members.FirstOrDefault()?.Value?.ToString();
				var dependencyProperty = TypeResolver.FindDependencyProperty(member);

				if (keyName != null
					&& dependencyProperty != null
					&& instance is DependencyObject dependencyObject)
				{
					ResourceResolver.ApplyResource(
						dependencyObject,
						dependencyProperty,
						keyName,
						isThemeResourceExtension: IsThemeResourceMarkupNode(member),
						isHotReloadSupported: true);

					if (instance is FrameworkElement fe)
					{
						fe.Loading += delegate
						{
							fe.UpdateResourceBindings();
						};
					}
				}
				else if (propertyInfo != null)
				{
					GetPropertySetter(propertyInfo).Invoke(
								instance,
								new[] { ResourceResolver.ResolveResourceStatic(keyName, propertyInfo.PropertyType) }
							);

					if (instance is Setter setter && propertyInfo.Name == "Value")
					{
						// Register StaticResource/ThemeResource assignations to Value for resource updates
						setter.ApplyThemeResourceUpdateValues(
							keyName,
							null,
							IsThemeResourceMarkupNode(member),
							true
						);
					}
				}
			}
		}

		private static object? ResolveStaticResource(object? instance, string? keyName)
		{
			var staticResource = (instance as FrameworkElement)
					.Flatten(i => (i?.Parent as FrameworkElement))
					.Select(fe =>
					{
						if (fe != null
							&& fe.Resources.TryGetValue(keyName, out var resource, shouldCheckSystem: false))
						{
							return resource;
						}
						return null;
					})
					.Concat(ResourceResolver.ResolveTopLevelResource(keyName))
					.Trim()
					.FirstOrDefault();

			return staticResource;
		}

		private bool IsStaticResourceMarkupNode(XamlMemberDefinition member)
			=> member.Objects.Any(o => o.Type.Name == "StaticResource");

		private bool IsThemeResourceMarkupNode(XamlMemberDefinition member)
			=> member.Objects.Any(o => o.Type.Name == "ThemeResource");

		private bool IsResourcesProperty(PropertyInfo propertyInfo)
			=> propertyInfo.Name == "Resources" && propertyInfo.PropertyType == typeof(ResourceDictionary);

		private void ProcessBindingMarkupNode(object instance, object? rootInstance, XamlMemberDefinition member)
		{
			var binding = BuildBindingExpression(instance, rootInstance, member);

			if (instance is IDependencyObjectStoreProvider provider)
			{
				var dependencyProperty = TypeResolver.FindDependencyProperty(member);

				if (dependencyProperty != null)
				{
					provider.Store.SetBinding(dependencyProperty, binding);
				}
				else if (TypeResolver.GetPropertyByName(member.Owner.Type, member.Member.Name) is PropertyInfo propertyInfo)
				{
					if (member.Objects.Empty())
					{
						GetPropertySetter(propertyInfo).Invoke(instance, new[] { BuildLiteralValue(member, propertyInfo.PropertyType) });
					}
					else
					{
						GetPropertySetter(propertyInfo).Invoke(instance, new[] { BuildBindingExpression(null, rootInstance, member) });
					}
				}
				else
				{
					throw new NotSupportedException($"Unknown dependency property {member.Member}");
				}
			}
			else
			{
				throw new NotSupportedException($"Binding is not supported on {member.Member}");
			}
		}

		private Binding BuildBindingExpression(object? instance, object? rootInstance, XamlMemberDefinition member)
		{
			var bindingNode = member.Objects.FirstOrDefault(o => o.Type.Name == "Binding");
			var templateBindingNode = member.Objects.FirstOrDefault(o => o.Type.Name == "TemplateBinding");
			var xBindNode = member.Objects.FirstOrDefault(o => o.Type.Name == "Bind");

			var binding = new Data.Binding();

			if (templateBindingNode != null)
			{
				binding.RelativeSource = RelativeSource.TemplatedParent;
			}

			if (xBindNode != null)
			{
				binding.Source = rootInstance;
				// TODO: here we should be setting Mode to OneTime by default, and we should also respect x:DefaultBindMode values set 
				// further up in the tree.
			}

			if(bindingNode == null && templateBindingNode == null && xBindNode == null)
			{
				throw new InvalidOperationException("Unable to find Binding or TemplateBinding or x:Bind node");
			}

			foreach (var bindingProperty in (bindingNode ?? templateBindingNode ?? xBindNode)!.Members)
			{
				switch (bindingProperty.Member.Name)
				{
					case "_PositionalParameters":
					case nameof(Binding.Path):
						binding.Path = RewriteAttachedPropertyPath(bindingProperty.Value?.ToString());
						break;

					case nameof(Binding.ElementName):
						var subject = new ElementNameSubject();
						binding.ElementName = subject;

						if (bindingProperty.Value?.ToString() is { } value)
						{
							AddElementName(value, subject);
						}
						break;

					case nameof(Binding.TargetNullValue):
						binding.TargetNullValue = bindingProperty.Value?.ToString();
						break;

					case nameof(Binding.FallbackValue):
						binding.FallbackValue = bindingProperty.Value?.ToString();
						break;

					case nameof(Binding.UpdateSourceTrigger):
						if (Enum.TryParse<UpdateSourceTrigger>(bindingProperty.Value?.ToString(), out var trigger))
						{
							binding.UpdateSourceTrigger = trigger;
						}
						else
						{
							throw new NotSupportedException($"Invalid binding mode {bindingProperty.Value}");
						}
						break;

					case nameof(Binding.RelativeSource):
						if (bindingProperty.Objects.First() is XamlObjectDefinition relativeSource && relativeSource.Type.Name == "RelativeSource")
						{
							var relativeSourceValue = relativeSource.Members.FirstOrDefault()?.Value?.ToString()?.ToLowerInvariant();
							switch (relativeSourceValue)
							{
								case "templatedparent":
									binding.RelativeSource = RelativeSource.TemplatedParent;
									break;

								default:
									throw new NotSupportedException($"RelativeSource {relativeSourceValue} is not supported");
							}
						}
						break;

					case nameof(Binding.Mode):
						if (Enum.TryParse<Data.BindingMode>(bindingProperty.Value?.ToString(), out var mode))
						{
							binding.Mode = mode;
						}
						else
						{
							throw new NotSupportedException($"Invalid binding mode {bindingProperty.Value}");
						}
						break;

					case nameof(Binding.ConverterParameter):
						binding.ConverterParameter = bindingProperty.Value?.ToString();
						break;

					case nameof(Binding.Converter):
						if (
							bindingProperty.Objects.First() is XamlObjectDefinition converterResource
						)
						{
							if (converterResource.Type.Name == "StaticResource")
							{
								var staticResourceName = converterResource.Members.FirstOrDefault()?.Value?.ToString();

								void ResolveResource()
								{
									var staticResource = ResolveStaticResource(instance, staticResourceName);

									if (staticResource != null)
									{
										binding.Converter = staticResource as IValueConverter;
									}
								}

								_postActions.Enqueue(ResolveResource);
							}
							else
							{
								throw new NotSupportedException($"Markup extension {converterResource.Type.Name} is not supported for Bindiner.Converter");
							}
						}
						break;

					default:
						throw new NotSupportedException($"Binding option {bindingProperty.Member} is not supported");
				}
			}

			return binding;
		}

		private void AddElementName(string elementName, ElementNameSubject subject)
		{
			_elementNames.Add((elementName, subject));
		}

		private bool IsBindingMarkupNode(XamlMemberDefinition member)
			=> member.Objects.Any(o => o.Type.Name == "Binding" || o.Type.Name == "TemplateBinding" || o.Type.Name == "Bind");

		private static bool IsMarkupExtension(XamlMemberDefinition member)
			=> member
				.Objects
				.Where(m =>
					m.Type.Name == "Binding"
					|| m.Type.Name == "Bind"
					|| m.Type.Name == "StaticResource"
					|| m.Type.Name == "ThemeResource"
					|| m.Type.Name == "TemplateBinding"
				)
				.Any();

		private void AddGenericDictionaryItems(
			IDictionary<object, object> dictionary,
			IEnumerable<XamlObjectDefinition> nonBindingObjects,
			object rootInstance)
		{
			foreach (var child in nonBindingObjects)
			{
				var item = LoadObject(child, rootInstance: rootInstance);

				var resourceKey = GetResourceKey(child);

				if (resourceKey != null && item != null)
				{
					dictionary[resourceKey] = item;
				}
			}
		}

		private void AddDictionaryItems(object collectionInstance, IEnumerable<XamlObjectDefinition> nonBindingObjects, object rootInstance)
		{
			MethodInfo? addMethodInfo = null;

			foreach (var child in nonBindingObjects)
			{
				var item = LoadObject(child, rootInstance: rootInstance);

				if (addMethodInfo == null)
				{
					addMethodInfo = collectionInstance
						.GetType()
						.GetMethods(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public)
						.Where(m => m.Name == "set_Item")
						.FirstOrDefault(m => m.GetParameters() is { Length: 1 } p
							&& (item?.GetType() ?? typeof(object)).Is(p[0].ParameterType))
						?? throw new InvalidOperationException($"The type {collectionInstance.GetType()} does not contains an Add({item?.GetType()}) method");
				}

				addMethodInfo.Invoke(collectionInstance, new[] { item });
			}
		}

		private void AddCollectionItems(object collectionInstance, IEnumerable<XamlObjectDefinition> nonBindingObjects, object rootInstance)
		{
			MethodInfo? addMethodInfo = null;

			foreach (var child in nonBindingObjects)
			{
				var item = LoadObject(child, rootInstance: rootInstance);

				if (addMethodInfo == null)
				{
					addMethodInfo = collectionInstance
						.GetType()
						.GetMethods(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public)
						.Where(m => m.Name == "Add")
						.FirstOrDefault(m => m.GetParameters() is { Length: 1 } p
							&& (item?.GetType() ?? typeof(object)).Is(p[0].ParameterType))
						?? throw new InvalidOperationException($"The type {collectionInstance.GetType()} does not contains an Add({item?.GetType()}) method");
				}

				addMethodInfo.Invoke(collectionInstance, new[] { item });
			}
		}

		private MethodInfo? FindAddMethod(object collectionInstance, Type? itemType)
		{
			return collectionInstance.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public)
				.Where(m => m.Name == "Add")
				.FirstOrDefault(m =>
					m.GetParameters() is { Length: 1 } p &&
					(itemType ?? typeof(object)).Is(p[0].ParameterType)
				);
		}

		private object? GetResourceKey(XamlObjectDefinition child) =>
			child.Members.FirstOrDefault(m =>
				string.Equals(m.Member.Name, "Name", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(m.Member.Name, "Key", StringComparison.OrdinalIgnoreCase)
			)
			?.Value
			?.ToString();

		private Type? GetResourceTargetType(XamlObjectDefinition child) =>
				TypeResolver.FindType(child.Members.FirstOrDefault(m =>
					string.Equals(m.Member.Name, "TargetType", StringComparison.OrdinalIgnoreCase)
				)
				?.Value
				?.ToString() ?? ""
			);

		private PropertyInfo? GetMemberProperty(XamlObjectDefinition control, XamlMemberDefinition member)
		{
			if (member.Member.Name == "_UnknownContent")
			{
				var property = TypeResolver.FindContentProperty(TypeResolver.FindType(control.Type));

				if (property == null)
				{
					throw new InvalidOperationException($"Implicit content is not supported on {control.Type}");
				}

				return property;
			}
			else
			{
				return TypeResolver.GetPropertyByName(control.Type, member.Member.Name);
			}
		}

		private EventInfo? GetMemberEvent(XamlObjectDefinition control, XamlMemberDefinition member)
		{
			return TypeResolver.GetEventByName(control.Type, member.Member.Name);
		}

		private object? BuildLiteralValue(XamlMemberDefinition member, Type? propertyType = null)
		{
			if (member.Objects.None())
			{
				var memberValue = member.Value?.ToString();

				propertyType = propertyType ?? TypeResolver.FindPropertyType(member.Member);

				if (propertyType != null)
				{
					if (propertyType == typeof(Type))
					{
						return TypeResolver.FindType(memberValue);
					}
					else if (propertyType == typeof(DependencyProperty) && member.Owner.Type.Name == "Setter")
					{
						var propertyOwner = _styleTargetTypeStack.Peek();

						if (TypeResolver.FindDependencyProperty(propertyOwner, memberValue) is DependencyProperty property)
						{
							return property;
						}
						else
						{
							throw new Exception($"The property {propertyOwner}.{memberValue} does not exist");
						}
					}
					else
					{
						return BuildLiteralValue(propertyType, memberValue);
					}
				}
				else
				{
					throw new Exception($"The property {member.Owner?.Type?.Name}.{member.Member?.Name} is unknown");
				}
			}
			else
			{
				var expression = member.Objects.First();

				throw new NotSupportedException("MarkupExtension {0} is not supported.".InvariantCultureFormat(expression.Type.Name));
			}
		}

		private object? BuildLiteralValue(Type propertyType, string? memberValue)
		{
			if (propertyType.GetCustomAttribute<CreateFromStringAttribute>() is { } createFromString)
			{
				var sourceType = propertyType;
				var methodName = createFromString.MethodName;
				if (createFromString.MethodName.Contains("."))
				{
					var splitIndex = createFromString.MethodName.LastIndexOf(".");
					var typeName = createFromString.MethodName.Substring(0, splitIndex);
					sourceType = TypeResolver.FindType(typeName);
					methodName = createFromString.MethodName.Substring(splitIndex + 1);
				}

				if (sourceType?.GetMethod(methodName) is { } conversionMethod && conversionMethod.IsStatic && !conversionMethod.IsPrivate)
				{
					try
					{
						return conversionMethod.Invoke(null, new object?[] { memberValue });
					}
					catch (Exception ex)
					{
						throw new XamlParseException("Executing [CreateFromString] method for type " + propertyType + " failed.", ex);
					}
				}
				else
				{
					throw new XamlParseException("Method referenced by [CreateFromString] cannot be found for " + propertyType);
				}
			}

			return Uno.UI.DataBinding.BindingPropertyHelper.Convert(() => propertyType, memberValue);
		}

		private void ApplyPostActions(object? instance)
		{
			if (instance is FrameworkElement fe)
			{
				ResolveElementNames(fe);
			}

			while (_postActions.Count != 0)
			{
				_postActions.Dequeue()();
			}
		}

		private void ResolveElementNames(FrameworkElement root)
		{
			foreach (var (elementName, bindingSubject) in _elementNames)
			{
				if (root.FindName(elementName) is DependencyObject element)
				{
					bindingSubject.ElementInstance = element;
				}
			}
		}

		private class EventHandlerWrapper
		{
			private readonly object _instance;
			private readonly MethodInfo _method;

			public EventHandlerWrapper(object instance, MethodInfo method)
			{
				_instance = instance;
				_method = method;
			}

			public void Handler2(object sender, object args)
			{
				_method.Invoke(_instance, Array.Empty<object>());
			}

			public void Handler1(object sender)
			{
				_method.Invoke(_instance, Array.Empty<object>());
			}
		}
	}
}
