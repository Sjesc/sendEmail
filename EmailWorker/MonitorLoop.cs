using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;
using RemesasAPI;
using RemesasAPI.Entities;

public class MonitorLoop
{

  private readonly IBackgroundTaskQueue _taskQueue;
  private readonly ILogger _logger;
  private readonly CancellationToken _cancellationToken;
  private readonly ApiContext _context;

  public bool IsMonitoring { get; set; }
  private readonly EmailConfiguration _emailConfig;



  public MonitorLoop(IBackgroundTaskQueue taskQueue,
      ILogger<MonitorLoop> logger,
      IHostApplicationLifetime applicationLifetime,
      ApiContext context,
      EmailConfiguration emailConfig
      )
  {
    _taskQueue = taskQueue;
    _logger = logger;
    _cancellationToken = applicationLifetime.ApplicationStopping;
    _context = context;
    _emailConfig = emailConfig;
  }

  public void StartMonitorLoop()
  {
    _logger.LogInformation("MonitorAsync Loop is starting.");

    // Run a console user input loop in a background thread
    Task.Run(async () => await MonitorAsync());
  }

  private async ValueTask MonitorAsync()
  {
    Console.WriteLine(DateTime.Now + " Loading Emails :)");

    var emails = await this._context.EmailQueue.Where(x => x.SentDate == null).ToListAsync();

    Console.WriteLine("Found " + emails.Count() + " emails");

    await Task.WhenAll(emails.Select(email =>
    {
      var workItem = BuildWorkItem(email);
      return _taskQueue.QueueBackgroundWorkItemAsync(workItem);
    }));


    Console.WriteLine(DateTime.Now + "Correos enviados");

    await Task.Delay(10000);

    await MonitorAsync();
  }

  private async Task SendEmail(MimeMessage mimeMessage)
  {
    using var client = new MailKit.Net.Smtp.SmtpClient();

    if (_emailConfig.Dev == null || mimeMessage.To.First().ToString().Contains("@unidigital.global"))
    {
      await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, MailKit.Security.SecureSocketOptions.None);
    }
    else
    {
      await client.ConnectAsync(_emailConfig.Dev.SmtpServer, _emailConfig.Dev.Port, true);
      await client.AuthenticateAsync(_emailConfig.Dev.UserName, _emailConfig.Dev.Password);
    }

    try
    {
      await client.SendAsync(mimeMessage);

    }

    finally
    {
      await client.DisconnectAsync(true);
      client.Dispose();
    }
  }

  private MimeMessage CreateMimeMessage(string to, string title, string content)
  {

    var body = $@"
            <div style=""padding: 64px 0;width: 100%;background-color: #f7f7f7;"">
                <div style=""max-width: 503px;margin:auto;background-color:white"">
                    <div style=""padding:12px;font-size:24px;background: #ff963b;color:  white;"">
                        <div>Unidigital</div>
                    </div>

                    <div style=""padding: 50px;"">

                        <div style=""font-size:20px;margin-bottom: 12px;"">{title}</div>

                        <div style=""text-align: center;font-size: 15px;"">{content}</div>

                    </div>

                    <div style=""color:gray;font-size:12px;text-align:center; padding: 16px 50px;"">
                        <div>
                            Saludos desde Remesas-Unidigital, una plataforma donde podras enviar, recibir y solicitar remesas a tus familiares y amigos fuera del pais.
                        </div>
                        <div>
                            Copyright © 2022 - Corporación Unidigital 1220, C.A. RIF J-40148330-5
                        </div>
                    </div>
                </div>
            </div>";

    var emailMessage = new MimeMessage();

    emailMessage.From.Add(new MailboxAddress(_emailConfig.From));
    emailMessage.To.Add(new MailboxAddress(to));
    emailMessage.Subject = title;
    emailMessage.Body = new TextPart("html") { Text = body };

    return emailMessage;
  }

  private Func<CancellationToken, Task> BuildWorkItem(EmailQueue email)
  {

    // Simulate three 5-second tasks to complete
    // for each enqueued work item

    var function = async (CancellationToken token) =>
    {
      Console.WriteLine("Enviando correo a " + email.ToEmail);

      if (!token.IsCancellationRequested)
      {
        try
        {
          var message = CreateMimeMessage(email.ToEmail, email.Subject, email.Body);

          await SendEmail(message);

          email.SentDate = DateTime.UtcNow;
          await this._context.SaveChangesAsync();


        }
        catch (OperationCanceledException)
        {
          // Prevent throwing if the Delay is cancelled
        }
      }


    };

    return function;
  }
}