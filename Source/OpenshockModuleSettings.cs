using Celeste.Mod.Openshock.menu;
using System.Collections.Generic;

namespace Celeste.Mod.Openshock;

public class OpenshockModuleSettings : EverestModuleSettings
{
	[SettingIgnore]
	public bool Toggle { get; private set; } = true;
	private TextMenu.OnOff ToggleEntry;

	[SettingIgnore]
	public bool DebugLog { get; private set; } = false;
	private TextMenu.OnOff DebugLogEntry;

	[SettingIgnore]
	public string ApiKey { get; private set; } = "";
	private ButtonInputString ApiKeyEntry;

	[SettingIgnore]
	public string ApiBaseUrl { get; private set; } = "https://api.openshock.app/";
	private ButtonInputString ApiBaseUrlEntry;

	public enum IntensityModeEnum
	{
		Range = 0,
		Static
	}
	[SettingIgnore]
	public IntensityModeEnum IntensityMode { get; private set; } = IntensityModeEnum.Range;
	private TextMenuExt.EnumSlider<IntensityModeEnum> IntensityModeEntry;

	[SettingIgnore]
	public int IntensityRangeMinimum { get; private set; } = 300;
	private TextMenuExt.IntSlider IntensityRangeMinimumEntry;

	[SettingIgnore]
	public int IntensityRangeMaximum { get; private set; } = 30000;
	private TextMenuExt.IntSlider IntensityRangeMaximumEntry;

	[SettingIgnore]
	public int IntensityStatic { get; private set; } = 300;
	private TextMenuExt.IntSlider IntensityStaticEntry;

	public enum DurationModeEnum
	{
		Range = 0,
		Static
	}
	[SettingIgnore]
	public DurationModeEnum DurationMode { get; private set; } = DurationModeEnum.Range;
	private TextMenuExt.EnumSlider<DurationModeEnum> DurationModeEntry;

	[SettingIgnore]
	public float DurationRangeMinimum { get; private set; } = 1.0f;
	private FloatSlider DurationRangeMinimumEntry;

	[SettingIgnore]
	public float DurationRangeMaximum { get; private set; } = 30.0f;
	private FloatSlider DurationRangeMaximumEntry;

	[SettingIgnore]
	public float DurationStatic { get; private set; } = 1.0f;
	private FloatSlider DurationStaticEntry;

	public enum ShockModeEnum
	{
		All = 0,
		Random,
		Single
	}
	[SettingIgnore]
	public ShockModeEnum ShockMode { get; private set; } = ShockModeEnum.All;
	private TextMenuExt.EnumSlider<ShockModeEnum> ShockModeEntry;

	[SettingIgnore]
	public string ShockerSingle { get; private set; } = string.Empty;
	private ButtonInputString ShockerSingleEntry;

	[SettingIgnore]
	public List<string> ShockerList { get; private set; } = new List<string>();
	private ButtonInputString ShockerListEntry;

	public void CreateToggleEntry(TextMenu menu, bool inGame)
	{
		if (inGame)
		{
			menu.Add(new TextMenu.SubHeader(
				label: "Some options may be disabled because you are currently in-game"
			));
		}

		// Global
		menu.Add(ToggleEntry = new TextMenu.OnOff(
			label: "Toggle",
			on: Toggle
		));
		menu.Add(DebugLogEntry = new TextMenu.OnOff(
			label: "Debug Log",
			on: DebugLog
		));

		// API settings
		menu.Add(ApiKeyEntry = new ButtonInputString(
			label: "API Key",
			value: ApiKey,
			maxLength: 512,
			onChanged: (value) => ApiKey = value,
			displayFormatter: (value) => new string('*', value.Length)
		));
		menu.Add(ApiBaseUrlEntry = new ButtonInputString(
			label: "API Base Url",
			value: ApiBaseUrl,
			maxLength: 512,
			onChanged: (value) => ApiBaseUrl = value
		));

		// Intensity settings
		menu.Add(IntensityModeEntry = new TextMenuExt.EnumSlider<IntensityModeEnum>(
			label: "Intensity Mode",
			startValue: IntensityMode
		));
		menu.Add(IntensityRangeMinimumEntry = new TextMenuExt.IntSlider(
			label: "Intensity Range Minimum",
			min: 1,
			max: 100,
			value: IntensityRangeMinimum
		));
		menu.Add(IntensityRangeMaximumEntry = new TextMenuExt.IntSlider(
			label: "Intensity Range Maximum",
			min: 1,
			max: 100,
			value: IntensityRangeMaximum
		));
		menu.Add(IntensityStaticEntry = new TextMenuExt.IntSlider(
			label: "Intensity",
			min: 1,
			max: 100,
			value: IntensityStatic
		));

		// Duration settings
		menu.Add(DurationModeEntry = new TextMenuExt.EnumSlider<DurationModeEnum>(
			label: "Duration Mode",
			startValue: DurationModeEnum.Range
		));
		menu.Add(DurationRangeMinimumEntry = new FloatSlider(
			label: "Duration Minimum",
			min: 0.3f,
			max: 30.0f,
			stepping: [0.1f, 0.5f, 1.0f, 5.0f],
			formatter: (v) => $"{float.Round(v, 1)}s",
			value: DurationRangeMinimum
		));
		menu.Add(DurationRangeMaximumEntry = new FloatSlider(
			label: "Duration Maximum",
			min: 0.3f,
			max: 30.0f,
			stepping: [0.1f, 0.5f, 1.0f, 5.0f],
			formatter: (v) => $"{float.Round(v, 1)}s",
			value: DurationRangeMaximum
		));
		menu.Add(DurationStaticEntry = new FloatSlider(
			label: "Duration",
			min: 0.3f,
			max: 30.0f,
			stepping: [0.1f, 0.5f, 1.0f, 5.0f],
			formatter: (v) => $"{float.Round(v, 1)}s",
			value: DurationStatic
		));

		// Device settings
		menu.Add(ShockModeEntry = new TextMenuExt.EnumSlider<ShockModeEnum>(
			label: "Shocker Mode",
			ShockMode
		));
		menu.Add(ShockerSingleEntry = new ButtonInputString(
			label: "Shocker ID",
			maxLength: 512,
			value: ShockerSingle,
			onChanged: (value) => ShockerSingle = value
		));
		// TODO: This is absolutely fucking horrible. We need some kind of customizable list to add and remove entries but its too much work
		menu.Add(ShockerListEntry = new ButtonInputString(
			label: "Shocker IDs (Comma Separated)",
			maxLength: 512,
			onChanged: (value) =>
			{
				ShockerList.Clear();

				var entries = value.Split(",");
				foreach (var entry in entries)
					ShockerList.Add(entry.Trim());
			},
			displayFormatter: (value) =>
			{
				return $"{ShockerList.Count} Shocker{(ShockerList.Count == 1 ? "" : "s")}";
			}
		));

		// Options that enable/disable other options
		ToggleEntry.Change(value =>
		{
			Toggle = value;

			if (value)
			{
				// Enabled all the base settings
				DebugLogEntry.Visible = true;
				ApiKeyEntry.Visible = true;
				ApiBaseUrlEntry.Visible = true;
				IntensityModeEntry.Visible = true;
				DurationModeEntry.Visible = true;
				ShockModeEntry.Visible = true;

				// Update all the toggles which toggle other options visibility
				IntensityModeEntry.OnValueChange(IntensityMode);
				DurationModeEntry.OnValueChange(DurationMode);
				ShockModeEntry.OnValueChange(ShockMode);
			}
			else
			{
				// Hide everything
				DebugLogEntry.Visible = false;
				ApiKeyEntry.Visible = false;
				ApiBaseUrlEntry.Visible = false;
				IntensityModeEntry.Visible = false;
				IntensityRangeMinimumEntry.Visible = false;
				IntensityRangeMaximumEntry.Visible = false;
				IntensityStaticEntry.Visible = false;
				DurationModeEntry.Visible = false;
				DurationRangeMinimumEntry.Visible = false;
				DurationRangeMaximumEntry.Visible = false;
				DurationStaticEntry.Visible = false;
				ShockModeEntry.Visible = false;
				ShockerSingleEntry.Visible = false;
				ShockerListEntry.Visible = false;
			}
		});

		IntensityModeEntry.Change(value =>
		{
			IntensityMode = value;

			if (IntensityMode == IntensityModeEnum.Range)
			{
				IntensityRangeMinimumEntry.Visible = true;
				IntensityRangeMaximumEntry.Visible = true;
				IntensityStaticEntry.Visible = false;
			}
			else
			{
				IntensityRangeMinimumEntry.Visible = false;
				IntensityRangeMaximumEntry.Visible = false;
				IntensityStaticEntry.Visible = true;
			}
		});

		DurationModeEntry.Change(value =>
		{
			DurationMode = value;

			if (DurationMode == DurationModeEnum.Range)
			{
				DurationRangeMinimumEntry.Visible = true;
				DurationRangeMaximumEntry.Visible = true;
				DurationStaticEntry.Visible = false;
			}
			else
			{
				DurationRangeMinimumEntry.Visible = false;
				DurationRangeMaximumEntry.Visible = false;
				DurationStaticEntry.Visible = true;
			}
		});

		ShockModeEntry.Change(value =>
		{
			ShockMode = value;

			if (ShockMode == ShockModeEnum.Single)
			{
				ShockerSingleEntry.Visible = true;
				ShockerListEntry.Visible = false;
			}
			else
			{
				ShockerSingleEntry.Visible = false;
				ShockerListEntry.Visible = true;
			}
		});
		ShockModeEntry.OnValueChange(ShockMode); // Run once so we initialise the visiblity

		// Clamping
		DurationRangeMinimumEntry.Change(value =>
		{
			DurationRangeMinimum = value;

			if (DurationRangeMinimumEntry.Value > DurationRangeMaximumEntry.Value)
			{
				DurationRangeMaximumEntry.Value = DurationRangeMinimumEntry.Value;
				DurationRangeMaximumEntry?.OnValueChange(DurationRangeMaximumEntry.Value);
			}
		});
		DurationRangeMaximumEntry.Change(value =>
		{
			DurationRangeMaximum = value;

			if (DurationRangeMaximumEntry.Value < DurationRangeMinimumEntry.Value)
			{
				DurationRangeMinimumEntry.Value = DurationRangeMaximumEntry.Value;
				DurationRangeMinimumEntry?.OnValueChange(DurationRangeMinimumEntry.Value);
			}
		});

		IntensityRangeMinimumEntry.Change(value =>
		{
			IntensityRangeMinimum = value;

			if (IntensityRangeMinimumEntry.Index > IntensityRangeMaximumEntry.Index)
			{
				IntensityRangeMaximumEntry.Index = IntensityRangeMinimumEntry.Index;
				IntensityRangeMaximumEntry?.OnValueChange(IntensityRangeMaximumEntry.Index);
			}
		});
		IntensityRangeMaximumEntry.Change(value =>
		{
			IntensityRangeMaximum = value;

			if (IntensityRangeMaximumEntry.Index < IntensityRangeMinimumEntry.Index)
			{
				IntensityRangeMinimumEntry.Index = IntensityRangeMaximumEntry.Index;
				IntensityRangeMinimumEntry?.OnValueChange(IntensityRangeMinimumEntry.Index);
			}
		});

		// Other value updates
		IntensityStaticEntry.Change(value =>
		{
			IntensityStatic = value;
		});
		DurationStaticEntry.Change(value =>
		{
			DurationStatic = value;
		});

		// Special case for things that crash
		ApiKeyEntry.Disabled = inGame;
		ApiBaseUrlEntry.Disabled = inGame;
		ShockerSingleEntry.Disabled = inGame;
		ShockerListEntry.Disabled = inGame;

		// Finally run all the visibility updates
		IntensityModeEntry.OnValueChange(IntensityMode);
		DurationModeEntry.OnValueChange(DurationMode);
		ToggleEntry.OnValueChange(Toggle);
	}
}
