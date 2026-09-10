using MediatR;
using Microsoft.Extensions.Logging;
using OlliBot.Application.HumbleBundle.Models;

namespace OlliBot.Application.HumbleBundle.ScanHumbleBundle;

public class ScanHumbleBundleHandler(IHumbleBundleScanner humbleBundleScanner, ILogger<ScanHumbleBundleHandler> logger) : IRequestHandler<ScanHumbleBundleCommand, ScanHumbleBundleResult>
{
    public async Task<ScanHumbleBundleResult> Handle(ScanHumbleBundleCommand command, CancellationToken ct = default)
    {
        IEnumerable<ScannedHumbleBundle> scanResult = await humbleBundleScanner.ScanAsync(command.BundleType, ct);

        return new ScanHumbleBundleResult(scanResult, true, null);
    }
}
