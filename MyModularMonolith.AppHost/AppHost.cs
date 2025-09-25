using Aspire.Hosting.MailDev;

var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sqlserver-password", secret: true);

var sqlServer = builder.AddSqlServer("sqlserver", password: sqlPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("MyModularMonolith");

var maildev = builder.AddMailDev("maildev", options => options
    .WithPorts(httpPort: 8025, smtpPort: 2525)
    .WithAuth("admin", "secret"));

var api = builder.AddProject<Projects.MyModularMonolith_Api>("api")
    .WithReference(sqlServer)
    .WithReference(maildev)
    .WaitFor(sqlServer);

builder.Build().Run();
