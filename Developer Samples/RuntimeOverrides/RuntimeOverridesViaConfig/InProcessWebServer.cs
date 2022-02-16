using System;
using System.Net;

namespace RuntimeOverridesViaConfig
{
	public sealed class InProcessWebServer : IDisposable
	{
		private const int HttpPort = 9595;

		private readonly Action<HttpListenerContext> _processRequest;
		private readonly HttpListener _httpListener;

		public InProcessWebServer(Action<HttpListenerContext> processRequest)
		{
			_processRequest = processRequest;
			_httpListener = new HttpListener { AuthenticationSchemes = AuthenticationSchemes.Anonymous };
			_httpListener.Prefixes.Add(RootUrl);
			_httpListener.Start();
			Result = _httpListener.BeginGetContext(WebRequestCallback, _httpListener);
		}

		public static string RootUrl => $"http://localhost:{HttpPort}/";

		public IAsyncResult Result { get; private set; }

		public HttpListenerRequest Request { get; private set; }

		public void Dispose()
		{
			if (_httpListener == null) return;

			lock (_httpListener)
			{
				try
				{
					((IDisposable)_httpListener).Dispose();
				}
				catch
				{
				}
			}
		}

		private void WebRequestCallback(IAsyncResult result)
		{
			// Avoid accessing HttpListener while/after it has been disposed
			lock (_httpListener)
			{
				if (!_httpListener.IsListening) return;

				try
				{
					HttpListenerContext context = _httpListener.EndGetContext(result);

					Request = context.Request;

					try
					{
						if (_processRequest == null)
						{
							context.Response.StatusCode = (int)HttpStatusCode.OK;
							context.Response.Headers.Add(HttpResponseHeader.ContentType, "text/plain");
						}
						else
						{
							_processRequest(context);
						}
					}
					finally
					{
						context.Response.OutputStream.Close();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[Server] - Error: {ex.Message}");
				}
				finally
				{
					// Set up the next context
					Result = _httpListener.BeginGetContext(WebRequestCallback, _httpListener);
				}
			}
		}
	}
}
