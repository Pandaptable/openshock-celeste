using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.Openshock.menu
{
	public class ButtonInputString : TextMenuExt.ButtonExt
	{
		public string Value;
		public int MaxLength;
		public int MaxDisplayLength;
		public Action<string> OnChanged;
		public Func<string, string> DisplayFormatter;

		public ButtonInputString(string label, string icon = null, string value = "", int maxLength = 32, int maxDisplayLength = 16, Action<string> onChanged = null, Func<string, string> displayFormatter = null) : base(label, icon)
		{
			Value = value;
			MaxLength = maxLength;
			MaxDisplayLength = maxDisplayLength;
			OnChanged = onChanged;
			DisplayFormatter = displayFormatter;

			Pressed(() =>
			{
				// Crashes when selected while ingame, idk why
				Container.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					Value,
					value =>
					{
						onChanged?.Invoke(value);
						Value = value;
					},
					MaxLength
				);
			});
		}

		// Action is called with the new value before `ButtonInputString#Value` is updated, so you can compare if needed
		public void Changed(Action<string> onChanged)
		{
			OnChanged = onChanged;
		}

		public string GetDisplayValue()
		{
			string rawValue = DisplayFormatter == null ? Value : DisplayFormatter.Invoke(Value);
			return Value.Length > MaxDisplayLength ? rawValue.Substring(0, MaxDisplayLength) + "..." : rawValue;
		}

		public override void Render(Vector2 position, bool highlighted)
		{
			position += Offset;
			float num = Container.Alpha * Alpha;
			Color color = (Disabled ? TextColorDisabled : (highlighted ? Container.HighlightColor : TextColor)) * num;
			Color strokeColor = Color.Black * (num * num * num);
			bool flag = Container.InnerContent == TextMenu.InnerContentMode.TwoColumn && !AlwaysCenter;
			Vector2 textPosition = position + (flag ? Vector2.Zero : new Vector2(Container.Width * 0.5f, 0f));
			Vector2 justify = (flag ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0.5f));
			TextMenuExt.DrawIcon(position, Icon, IconWidth, Height(), IconOutline, (Disabled ? Color.DarkSlateGray : (highlighted ? Color.White : Color.LightSlateGray)) * num, ref textPosition);
			ActiveFont.DrawOutline(Label, textPosition, justify, Scale, color, 2f, strokeColor);

			float rightWidth = RightWidth();
			ActiveFont.DrawOutline(GetDisplayValue(), textPosition + new Vector2(Container.Width - rightWidth * 0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, color, 2f, strokeColor);
		}

		public override float RightWidth()
		{
			return ActiveFont.Measure(GetDisplayValue()).X;
		}
	}
}
