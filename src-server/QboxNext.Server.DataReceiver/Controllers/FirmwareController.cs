using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Utils;
using QBoxNext.Server.Business.Interfaces.Public;

namespace QboxNext.Server.DataReceiver.Controllers
{
    [ApiController]
    public class FirmwareController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly ILogger<FirmwareController> _logger;

        public FirmwareController(
            [NotNull] IRegistrationService registrationService,
            [NotNull] ILogger<FirmwareController> logger
        )
        {
            Guard.IsNotNull(registrationService, nameof(registrationService));
            Guard.IsNotNull(logger, nameof(logger));

            _registrationService = registrationService;
            _logger = logger;
        }

        // GET firmware/qbox
        // Example: /firmware/qbox/6618-1200-2305/12-45-001-687/R37
        //          /firmware/qbox/6618-1400-0200/15-46-001-414/R48
        [HttpGet(@"/firmware/qbox/{productNumber:regex(^\d{{4}}-\d{{4}}-\d{{4}}$)}/{serialNumber:regex(^\d{{2}}-\d{{2}}-\d{{3}}-\d{{3}}$)}/{rn}")]
        public async Task<ActionResult> GetAsync([NotNull] string productNumber, [NotNull] string serialNumber, [NotNull] string rn)
        {
            Guard.IsNotNullOrEmpty(productNumber, nameof(productNumber));
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));

            _logger.LogInformation("GetAsync /firmware/qbox/{productNumber}/{serialNumber}/{rn}", productNumber, serialNumber, rn);

            var details = await _registrationService.GetQboxRegistrationDetailsAsync(serialNumber);
            if (details == null)
            {
                _logger.LogWarning("SerialNumber {serialNumber} is not registered.", serialNumber);
                return BadRequest();
            }

            if (!details.FirmwareDownloadAllowed)
            {
                _logger.LogWarning("FirmwareDownload for SerialNumber {serialNumber} is not allowed.", serialNumber);
                return BadRequest();
            }

            if (string.IsNullOrEmpty(details.Firmware))
            {
                _logger.LogWarning("Getting the Firmware version based on the SerialNumber is yet supported.");
                return BadRequest();
            }

            _logger.LogInformation("Using Firmware '{firmware}'", details.Firmware);

            // TODO : move this code to business project
            string firmwarePath = Path.Join("firmware", details.Firmware);
            byte[] bytes = System.IO.File.ReadAllBytes(firmwarePath);
            return File(bytes, "application/octet-stream", "A48_ENCRYPT_ON_svn_ver_681");
        }
    }
}