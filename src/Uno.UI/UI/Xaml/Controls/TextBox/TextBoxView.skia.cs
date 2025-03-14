﻿#nullable enable

using System;

using Uno.Extensions;
using Uno.Foundation.Extensibility;
using Uno.Foundation.Logging;
using Uno.UI.Xaml.Controls.Extensions;
using Windows.UI.Xaml.Media;

namespace Windows.UI.Xaml.Controls
{
	internal class TextBoxView
	{
		private readonly ITextBoxViewExtension? _textBoxExtension;

		private readonly WeakReference<TextBox> _textBox;
		private readonly bool _isPasswordBox;
		private bool _isPasswordRevealed;

		public TextBoxView(TextBox textBox)
		{
			_textBox = new WeakReference<TextBox>(textBox);
			_isPasswordBox = textBox is PasswordBox;
			if (!ApiExtensibility.CreateInstance(this, out _textBoxExtension))
			{
				if (this.Log().IsEnabled(LogLevel.Warning))
				{
					this.Log().LogWarning(
						"No TextBoxView implementation is available " +
						"for this Skia target. Functionality will be limited.");
				}
			}
		}

		internal ITextBoxViewExtension? Extension => _textBoxExtension;

		public TextBox? TextBox
		{
			get
			{
				if (_textBox.TryGetTarget(out var target))
				{
					return target;
				}
				return null;
			}
		}

		internal int GetSelectionStart() => _textBoxExtension?.GetSelectionStart() ?? 0;

		internal int GetSelectionLength() => _textBoxExtension?.GetSelectionLength() ?? 0;

		public TextBlock DisplayBlock { get; } = new TextBlock();

		internal void SetTextNative(string text)
		{
			// TODO: Inheritance hierarchy is wrong in Uno. PasswordBox shouldn't inherit TextBox.
			// This needs to be moved to PasswordBox if it's separated from TextBox.
			if (_isPasswordBox && !_isPasswordRevealed)
			{
				// TODO: PasswordChar isn't currently implemented. It should be used here when implemented.
				DisplayBlock.Text = new string('•', text.Length);
			}
			else
			{
				DisplayBlock.Text = text;
			}

			_textBoxExtension?.SetTextNative(text);
		}

		internal void Select(int start, int length)
		{
			_textBoxExtension?.Select(start, length);
		}

		internal void OnForegroundChanged(Brush brush)
		{
			DisplayBlock.Foreground = brush;
			_textBoxExtension?.SetForeground(brush);
		}

		internal void OnFocusStateChanged(FocusState focusState)
		{
			if (focusState != FocusState.Unfocused)
			{
				DisplayBlock.Opacity = 0;
				_textBoxExtension?.StartEntry();
			}
			else
			{
				_textBoxExtension?.EndEntry();
				DisplayBlock.Opacity = 1;
			}
		}

		internal void SetIsPassword(bool isPassword)
		{
			_isPasswordRevealed = !isPassword;
			_textBoxExtension?.SetIsPassword(isPassword);
		}

		internal void UpdateTextFromNative(string newText)
		{
			var textBox = _textBox?.GetTarget();
			if (textBox != null)
			{
				var text = textBox.ProcessTextInput(newText);
				if (text != newText)
				{
					SetTextNative(text);
				}
			}
		}

		public void UpdateMaxLength() => _textBoxExtension?.UpdateNativeView();
	}
}
