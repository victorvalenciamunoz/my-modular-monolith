namespace MyModularMonolith.AdminUI;

public class BackendHttpClient
{
    public HttpClient HttpClient { get; }

    public BackendHttpClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }
}
