using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.Openshock;

public class OpenshockModule : EverestModule
{
	public static OpenshockModule Instance { get; private set; }

	public override Type SettingsType => typeof(OpenshockModuleSettings);
	public static OpenshockModuleSettings Settings => (OpenshockModuleSettings)Instance._Settings;

	public override Type SessionType => typeof(OpenshockModuleSession);
	public static OpenshockModuleSession Session => (OpenshockModuleSession)Instance._Session;

	public override Type SaveDataType => typeof(OpenshockModuleSaveData);
	public static OpenshockModuleSaveData SaveData => (OpenshockModuleSaveData)Instance._SaveData;
	private Random random = new Random();
	private int currentIncrementalIntensity = 0;

	public OpenshockModule()
	{
		Instance = this;
#if DEBUG
		// debug builds use verbose logging
		Logger.SetLogLevel(nameof(OpenshockModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(OpenshockModule), LogLevel.Info);
#endif
	}

	public override void Load()
	{
		Everest.Events.Player.OnDie += OnDie;
	}

	public override void Unload()
	{
		Everest.Events.Player.OnDie -= OnDie;
	}

	private void OnDie(Player p)
	{
		if (!Settings.Toggle)
		{
			Logger.Info(nameof(OpenshockModule), "Player died but mod is disabled");
			return;
		}

		if (string.IsNullOrWhiteSpace(Settings.ApiKey))
		{
			Logger.Warn(nameof(OpenshockModule), "Player died but API key is not set");
			return;
		}

		List<OpenshockApi.Shock> shocks = new List<OpenshockApi.Shock>();

		// First we collect all the IDs we want to shock
		switch (Settings.ShockMode)
		{
			case OpenshockModuleSettings.ShockModeEnum.All:
			{
				foreach (string id in Settings.ShockerList)
				{
					shocks.Add(new OpenshockApi.Shock
					{
						id = id
					});
				}
				break;
			}
			case OpenshockModuleSettings.ShockModeEnum.Random:
			{
				if (Settings.ShockerList.Count > 0)
				{
					int index = random.Next(Settings.ShockerList.Count);
					shocks.Add(new OpenshockApi.Shock
					{
						id = Settings.ShockerList[index]
					});
				}
				break;
			}
			case OpenshockModuleSettings.ShockModeEnum.Single:
			{
				shocks.Add(new OpenshockApi.Shock
				{
					id = Settings.ShockerSingle
				});
				break;
			}
		}

		// For each of them select intensity and duration
		foreach (var shock in shocks)
		{
			shock.type = OpenshockApi.ControlType.Shock;
			shock.intensity = GetIntensity();
			shock.duration = GetDurationMs();
		}

		// Attack
		Logger.Info(nameof(OpenshockModule), $"Sending for {shocks.Count} device{(shocks.Count == 1 ? "" : "s")}");
		if (shocks.Count > 0)
			OpenshockApi.Send(shocks);
	}

	private int GetIntensity()
	{
		switch (Settings.IntensityMode)
		{
			case OpenshockModuleSettings.IntensityModeEnum.Range:
				return random.Next(Settings.IntensityRangeMinimum, Settings.IntensityRangeMaximum);
			case OpenshockModuleSettings.IntensityModeEnum.Static:
				return Settings.IntensityStatic;
			case OpenshockModuleSettings.IntensityModeEnum.Incremental:
				// Initialize if this is the first time or if settings changed
				if (currentIncrementalIntensity == 0)
				{
					currentIncrementalIntensity = Settings.IntensityRangeMinimum;
				}
				
				int intensity = currentIncrementalIntensity;
				
				// Increment for next time, but cap at maximum
				if (currentIncrementalIntensity < Settings.IntensityRangeMaximum)
				{
					currentIncrementalIntensity++;
				}
				
				return intensity;
			default:
				throw new ArgumentOutOfRangeException("IntensityMode is set unreasonably");
		}
	}

	private int GetDurationMs()
	{
		switch (Settings.DurationMode)
		{
			case OpenshockModuleSettings.DurationModeEnum.Range:
			{
				float value = random.NextFloat() * (Settings.DurationRangeMaximum - Settings.DurationRangeMinimum) + Settings.DurationRangeMinimum;
				return (int)(float.Round(value, 1) * 1000.0f);
			}
			case OpenshockModuleSettings.DurationModeEnum.Static:
			{
				return (int)Math.Floor(float.Round(Settings.DurationStatic, 1) * 1000.0f);
			}
			default:
				throw new ArgumentOutOfRangeException("DurationMode is set unreasonably");
		}
	}
}
