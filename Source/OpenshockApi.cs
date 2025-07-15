using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Celeste.Mod.Openshock
{
	public class OpenshockApi
	{
		public enum ControlType : int
		{
			Stop = 0,
			Shock,
			Vibrate,
			Sound
		}

		public class Shock
		{
			public string id { get; set; }
			public ControlType type { get; set; }
			public int intensity { get; set; }
			public int duration { get; set; }
		}

		private class Request
		{
			public List<Shock> shocks { get; set; }
			public string customName { get; set; }
		}

		private static HttpClient client = null;

		public static void Send(List<Shock> shocks)
		{
			if (client == null)
			{
			    // First time initializing
			    client = new HttpClient();
			    client.DefaultRequestHeaders.Add("OpenShockToken", OpenshockModule.Settings.ApiKey);
			    client.DefaultRequestHeaders.Add("User-Agent", "Celeste-OpenShock-Mod/1.0");
			}

			UriBuilder builder = new UriBuilder(OpenshockModule.Settings.ApiBaseUrl);
			builder.Path = "/2/shockers/control";

			Request request = new Request
			{
				shocks = shocks,
				customName = "Celeste Openshock"
			};
			string json = JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.Web));
			Uri uri = builder.Uri;

			if (OpenshockModule.Settings.DebugLog)
			{
				Logger.Info(nameof(OpenshockModule), $"POST {uri}");
				foreach (var header in client.DefaultRequestHeaders)
				{
					foreach (var value in header.Value)
					{
						// Censor the API key
						string display = value;
						if (header.Key == "OpenShockToken")
							display = new string('*', value.Length);

						Logger.Info(nameof(OpenshockModule), $"- {header.Key}: {display}");
					}
				}

				Logger.Info(nameof(OpenshockModule), "");
				Logger.Info(nameof(OpenshockModule), json);
			}

			var task = client.PostAsync(
				uri,
				new StringContent(json, Encoding.UTF8, "application/json")
			);
			task.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					Logger.Error(nameof(OpenshockModule), $"Failed to send shock to '{uri}': {t.Exception?.GetBaseException().Message}");
				}
				else if (t.Result.StatusCode != HttpStatusCode.OK)
				{
					Logger.Error(nameof(OpenshockModule), $"Failed to send shock to '{uri}': Status Code {t.Result.StatusCode}");

					var task2 = t.Result.Content.ReadAsStringAsync();
					task2.ContinueWith(t2 =>
					{
						if (t2.IsFaulted)
							Logger.Error(nameof(OpenshockModule), "=> *Failed to read server response*");
						else
							Logger.Error(nameof(OpenshockModule), $"=> {t2.Result}");
					}, TaskScheduler.Current);
				}
				else
				{
					Logger.Info(nameof(OpenshockModule), $"Successfully sent shock to '{uri}'");
				}
			}, TaskScheduler.Current);
		}
	}
}
