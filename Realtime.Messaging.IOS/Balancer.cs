using System;
using System.Threading.Tasks;
using ModernHttpClient;
using System.Net.Http;
using Foundation;
using Realtime.Messaging.Ext;
using Balancer = Realtime.Messaging.IOS.Balancer;

[assembly: Xamarin.Forms.Dependency(typeof(Balancer))]

namespace Realtime.Messaging.IOS
{
	[Preserve]
	public class Balancer : IBalancer
	{
		public Balancer ()
		{
		}

		public async Task<Tuple<Boolean,String>> ResolveClusterUrlAsync (String clusterUrl)
		{
			HttpClient aClient = new HttpClient(new NativeMessageHandler());
			Uri requestUri = new Uri(clusterUrl);

			var response = await aClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead);
			var server = await response.Content.ReadAsStringAsync();

			return new Tuple<bool,String>(response.IsSuccessStatusCode,server);
		}
	}
}

