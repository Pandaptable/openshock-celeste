using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.Openshock.menu
{
	public class FloatSlider : TextMenu.Item
	{
		public string Label;

		public float Value;

		public Action<float> OnValueChange;

		public float PreviousValue;

		private float sine;

		private int lastDir;

		private float min;

		private float max;
		private float[] stepping;
		private Func<float, string> formatter;

		private float fastMoveTimer;

		public FloatSlider(string label, float min, float max, float[] stepping, Func<float, string> formatter, float value = 0.0f)
		{
			Label = label;
			Selectable = true;
			this.min = min;
			this.max = max;
			this.stepping = stepping;
			this.formatter = formatter;
			Value = ((value < min) ? min : ((value > max) ? max : value));
		}

		public FloatSlider Change(Action<float> action)
		{
			OnValueChange = action;
			return this;
		}

		public override void Added()
		{
			Container.InnerContent = TextMenu.InnerContentMode.TwoColumn;
		}

		public override void LeftPressed()
		{
			if (Input.MenuLeft.Repeating)
			{
				fastMoveTimer += Engine.RawDeltaTime * 8f;
			}
			else
			{
				fastMoveTimer = 0f;
			}

			if (Value > min)
			{
				Audio.Play("event:/ui/main/button_toggle_off");
				PreviousValue = Value;
				int fastMoveSteppingIndex = (int)Math.Floor(fastMoveTimer);
				Value -= stepping[fastMoveSteppingIndex >= stepping.Length ? stepping.Length - 1 : fastMoveSteppingIndex];
				Value = Math.Max(min, Value);
				lastDir = -1;
				ValueWiggler.Start();
				OnValueChange?.Invoke(Value);
			}
		}

		public override void RightPressed()
		{
			if (Input.MenuRight.Repeating)
			{
				fastMoveTimer += Engine.RawDeltaTime * 8f;
			}
			else
			{
				fastMoveTimer = 0f;
			}

			if (Value < max)
			{
				Audio.Play("event:/ui/main/button_toggle_on");
				PreviousValue = Value;
				int fastMoveSteppingIndex = (int)Math.Floor(fastMoveTimer);
				Value += stepping[fastMoveSteppingIndex >= stepping.Length ? stepping.Length - 1 : fastMoveSteppingIndex];
				Value = Math.Min(max, Value);
				lastDir = 1;
				ValueWiggler.Start();
				OnValueChange?.Invoke(Value);
			}
		}

		public override void ConfirmPressed()
		{
			if (max - min == 1)
			{
				if (Value == min)
				{
					Audio.Play("event:/ui/main/button_toggle_on");
				}
				else
				{
					Audio.Play("event:/ui/main/button_toggle_off");
				}

				PreviousValue = Value;
				lastDir = ((Value == min) ? 1 : (-1));
				Value = ((Value == min) ? max : min);
				ValueWiggler.Start();
				OnValueChange?.Invoke(Value);
			}
		}

		public override void Update()
		{
			sine += Engine.RawDeltaTime;
		}

		public override float LeftWidth()
		{
			return ActiveFont.Measure(Label).X + 32f;
		}

		public override float RightWidth()
		{
			return Calc.Max(0f, ActiveFont.Measure(max.ToString()).X, ActiveFont.Measure(min.ToString()).X, ActiveFont.Measure(formatter.Invoke(Value)).X) + 120f;
		}

		public override float Height()
		{
			return ActiveFont.LineHeight;
		}

		public override void Render(Vector2 position, bool highlighted)
		{
			float alpha = Container.Alpha;
			Color strokeColor = Color.Black * (alpha * alpha * alpha);
			Color color = Disabled ? Color.DarkSlateGray : ((highlighted ? Container.HighlightColor : Color.White) * alpha);
			ActiveFont.DrawOutline(Label, position, new Vector2(0f, 0.5f), Vector2.One, color, 2f, strokeColor);
			if (max - min > 0)
			{
				float num = RightWidth();
				ActiveFont.DrawOutline(formatter.Invoke(Value), position + new Vector2(Container.Width - num * 0.5f + lastDir * ValueWiggler.Value * 8f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, color, 2f, strokeColor);
				Vector2 vector = Vector2.UnitX * (float)(highlighted ? (Math.Sin(sine * 4f) * 4.0) : 0.0);
				Vector2 position2 = position + new Vector2(Container.Width - num + 40f + ((lastDir < 0) ? ((0f - ValueWiggler.Value) * 8f) : 0f), 0f) - ((Value > min) ? vector : Vector2.Zero);
				ActiveFont.DrawOutline("<", position2, new Vector2(0.5f, 0.5f), Vector2.One, (Value > min) ? color : (Color.DarkSlateGray * alpha), 2f, strokeColor);
				position2 = position + new Vector2(Container.Width - 40f + ((lastDir > 0) ? (ValueWiggler.Value * 8f) : 0f), 0f) + ((Value < max) ? vector : Vector2.Zero);
				ActiveFont.DrawOutline(">", position2, new Vector2(0.5f, 0.5f), Vector2.One, (Value < max) ? color : (Color.DarkSlateGray * alpha), 2f, strokeColor);
			}
		}
	}
}
