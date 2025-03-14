﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.UI.RuntimeTests.Helpers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static Private.Infrastructure.TestServices;
using Windows.UI.Xaml;
using Windows.UI;
#if NETFX_CORE
using Uno.UI.Extensions;
#elif __IOS__
using UIKit;
#elif __MACOS__
using AppKit;
#else
#endif

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls
{
	[TestClass]
	[RunsOnUIThread]
	public class Given_TextBox
	{
#if __ANDROID__
		[TestMethod]
		public void When_InputScope_Null_And_ImeOptions()
		{
			var tb = new TextBox();
			tb.InputScope = null;
#if !MONOANDROID80
			tb.ImeOptions = Android.Views.InputMethods.ImeAction.Search;
#endif
		}
#endif

#if HAS_UNO
		[TestMethod]
		public async Task When_Template_Recycled()
		{
			try
			{
				var textBox = new TextBox
				{
					Text = "Test"
				};

				WindowHelper.WindowContent = textBox;
				await WindowHelper.WaitForLoaded(textBox);

				FocusManager.GettingFocus += OnGettingFocus;
				textBox.OnTemplateRecycled();
			}
			finally
			{
				FocusManager.GettingFocus -= OnGettingFocus;
			}

			static void OnGettingFocus(object sender, GettingFocusEventArgs args)
			{
				Assert.Fail("Focus should not move");
			}
		}
#endif

		[TestMethod]
		public async Task When_Fluent_And_Theme_Changed()
		{
			using (StyleHelper.UseFluentStyles())
			{
				var textBox = new TextBox
				{
					PlaceholderText = "Enter..."
				};

				WindowHelper.WindowContent = textBox;
				await WindowHelper.WaitForLoaded(textBox);

				var placeholderTextContentPresenter = textBox.FindFirstChild<TextBlock>(tb => tb.Name == "PlaceholderTextContentPresenter");
				Assert.IsNotNull(placeholderTextContentPresenter);

				var lightThemeForeground = TestsColorHelper.ToColor("#9E000000");
				var darkThemeForeground = TestsColorHelper.ToColor("#C5FFFFFF");

				Assert.AreEqual(lightThemeForeground, (placeholderTextContentPresenter.Foreground as SolidColorBrush)?.Color);

				using (ThemeHelper.UseDarkTheme())
				{
					Assert.AreEqual(darkThemeForeground, (placeholderTextContentPresenter.Foreground as SolidColorBrush)?.Color);
				}

				Assert.AreEqual(lightThemeForeground, (placeholderTextContentPresenter.Foreground as SolidColorBrush)?.Color);
			}
		}

		[TestMethod]
		public async Task When_BeforeTextChanging()
		{
			var textBox = new TextBox
			{
				Text = "Test"
			};

			WindowHelper.WindowContent = textBox;
			await WindowHelper.WaitForLoaded(textBox);

			textBox.BeforeTextChanging += (s, e) =>
			{
				Assert.AreEqual("Test", textBox.Text);
			};

			textBox.Text = "Something";
		}

		[TestMethod]
		public async Task When_Calling_Select_With_Negative_Values()
		{
			var textBox = new TextBox();
			WindowHelper.WindowContent = textBox;
			await WindowHelper.WaitForLoaded(textBox);

			Assert.ThrowsException<ArgumentException>(() => textBox.Select(0, -1));
			Assert.ThrowsException<ArgumentException>(() => textBox.Select(-1, 0));
		}

		[TestMethod]
		public async Task When_Calling_Select_With_In_Range_Values()
		{
			var textBox = new TextBox
			{
				Text = "0123456789"
			};

			WindowHelper.WindowContent = textBox;
			await WindowHelper.WaitForLoaded(textBox);
#if __WASM__ // Wasm is behaving differently than UWP and other platforms. https://github.com/unoplatform/uno/issues/7016
			Assert.AreEqual(10, textBox.SelectionStart);
#else
			Assert.AreEqual(0, textBox.SelectionStart);
#endif

			Assert.AreEqual(0, textBox.SelectionLength);
			textBox.Select(1, 7);
			Assert.AreEqual(1, textBox.SelectionStart);
			Assert.AreEqual(7, textBox.SelectionLength);
		}

		[TestMethod]
		public async Task When_Calling_Select_With_Out_Of_Range_Length()
		{
			var textBox = new TextBox
			{
				Text = "0123456789"
			};

			WindowHelper.WindowContent = textBox;
			await WindowHelper.WaitForLoaded(textBox);

#if __WASM__ // Wasm is behaving differently than UWP and other platforms. https://github.com/unoplatform/uno/issues/7016
			Assert.AreEqual(10, textBox.SelectionStart);
#else
			Assert.AreEqual(0, textBox.SelectionStart);
#endif
			Assert.AreEqual(0, textBox.SelectionLength);
			textBox.Select(1, 20);
			Assert.AreEqual(1, textBox.SelectionStart);
			Assert.AreEqual(9, textBox.SelectionLength);
		}

		[TestMethod]
		public async Task When_Calling_Select_With_Out_Of_Range_Start()
		{
			var textBox = new TextBox
			{
				Text = "0123456789"
			};

			WindowHelper.WindowContent = textBox;
			await WindowHelper.WaitForLoaded(textBox);

#if __WASM__ // Wasm is behaving differently than UWP and other platforms. https://github.com/unoplatform/uno/issues/7016
			Assert.AreEqual(10, textBox.SelectionStart);
#else
			Assert.AreEqual(0, textBox.SelectionStart);
#endif
			Assert.AreEqual(0, textBox.SelectionLength);
			textBox.Select(20, 5);
			Assert.AreEqual(10, textBox.SelectionStart);
			Assert.AreEqual(0, textBox.SelectionLength);
		}

#if __IOS__
		[Ignore("Disabled as not working properly. See https://github.com/unoplatform/uno/issues/8016")]
#endif
		[TestMethod]
		public async Task When_SelectionStart_Set()
		{
			var textBox = new TextBox
			{
				Text = "0123456789"
			};

			var button = new Button()
			{
				Content = "Some button"
			};

			var stackPanel = new StackPanel()
			{
				Children =
				{
					textBox,
					button
				}
			};

			WindowHelper.WindowContent = stackPanel;
			await WindowHelper.WaitForLoaded(textBox);

			button.Focus(FocusState.Programmatic);

			await WindowHelper.WaitForIdle();

			textBox.SelectionStart = 3;

			textBox.Focus(FocusState.Programmatic);
			Assert.AreEqual(3, textBox.SelectionStart);
		}

#if __IOS__
		[Ignore("Disabled as not working properly. See https://github.com/unoplatform/uno/issues/8016")]
#endif
		[TestMethod]
		public async Task When_Focus_Changes_SelectionStart_Preserved()
		{
			var textBox = new TextBox
			{
				Text = "0123456789"
			};

			var button = new Button()
			{
				Content = "Some button"
			};

			var stackPanel = new StackPanel()
			{
				Children =
				{
					textBox,
					button
				}
			};

			WindowHelper.WindowContent = stackPanel;
			await WindowHelper.WaitForLoaded(textBox);

			textBox.Focus(FocusState.Programmatic);

			await WindowHelper.WaitForIdle();

			textBox.SelectionStart = 3;

			button.Focus(FocusState.Programmatic);
			Assert.AreEqual(3, textBox.SelectionStart);

			textBox.Focus(FocusState.Programmatic);
			Assert.AreEqual(3, textBox.SelectionStart);
		}

		[TestMethod]
		public async Task When_IsEnabled_Set()
		{
			var foregroundColor = new SolidColorBrush(Colors.Red);
			var disabledColor = new SolidColorBrush(Colors.Blue);

			var textbox = new TextBox
			{
				Text = "Original Text",
				Foreground = foregroundColor,
				Style = TestsResourceHelper.GetResource<Style>("MaterialOutlinedTextBoxStyle"),
				IsEnabled = false
			};

			var stackPanel = new StackPanel()
			{
				Children = { textbox }
			};


			WindowHelper.WindowContent = stackPanel;
			await WindowHelper.WaitForLoaded(textbox);


			var contentPresenter = (ScrollViewer)textbox.FindName("ContentElement");

			await WindowHelper.WaitForIdle();

			Assert.IsFalse(textbox.IsEnabled);
			Assert.AreEqual(contentPresenter.Foreground, disabledColor);

			textbox.IsEnabled = true;

			Assert.IsTrue(textbox.IsEnabled);
			Assert.AreEqual(contentPresenter.Foreground, foregroundColor);
		}

		[TestMethod]
		public async Task When_SelectedText_StartZero()
		{
			var textBox = new TextBox
			{
				Text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
			};

			WindowHelper.WindowContent = textBox;
			await WindowHelper.WaitForLoaded(textBox);

			textBox.Focus(FocusState.Programmatic);

			textBox.SelectionStart = 0;
			textBox.SelectionLength = 0;
			textBox.SelectedText = "1234";

			Assert.AreEqual("1234ABCDEFGHIJKLMNOPQRSTUVWXYZ", textBox.Text);
		}

		[TestMethod]
		public async Task When_SelectedText_EndOfText()
		{
			var textBox = new TextBox
			{
				Text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
			};

			WindowHelper.WindowContent = textBox;
			await WindowHelper.WaitForLoaded(textBox);

			textBox.Focus(FocusState.Programmatic);

			textBox.SelectionStart = 26;
			textBox.SelectedText = "1234";

			Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234", textBox.Text);
		}

		[TestMethod]
		public async Task When_SelectedText_MiddleOfText()
		{
			var textBox = new TextBox
			{
				Text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
			};

			WindowHelper.WindowContent = textBox;
			await WindowHelper.WaitForLoaded(textBox);

			textBox.Focus(FocusState.Programmatic);

			textBox.SelectionStart = 2;
			textBox.SelectionLength = 22;
			textBox.SelectedText = "1234";

			Assert.AreEqual("AB1234YZ", textBox.Text);
		}

		[TestMethod]
		public async Task When_SelectedText_AllTextToEmpty()
		{
			var textBox = new TextBox
			{
				Text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
			};

			WindowHelper.WindowContent = textBox;
			await WindowHelper.WaitForLoaded(textBox);

			textBox.Focus(FocusState.Programmatic);

			textBox.SelectionStart = 0;
			textBox.SelectionLength = 26;
			textBox.SelectedText = String.Empty;

			Assert.AreEqual(String.Empty, textBox.Text);
			Assert.AreEqual(0, textBox.SelectionStart);
			Assert.AreEqual(0, textBox.SelectionLength);
		}
	}
}
