using System.Drawing;
using AssistantLibrary.Interfaces;
using AssistantLibrary.Models;
using HelperLibrary.Shared;
using Microsoft.Extensions.Options;
using QRCoder;
using static QRCoder.QRCodeGenerator;

namespace AssistantLibrary.Services {

    public sealed class QrCodeMaker : IQrCodeMaker {

        private readonly int _size;
        private readonly string _darkColor;
        private readonly string _lightColor;
        private readonly ECCLevel _eccLevel;
        private readonly bool _withLogo;
        private readonly string _logoName;

        private readonly QRCodeGenerator _generator;

        public QrCodeMaker(IOptions<QrCodeOptions> options) {
            _size = int.Parse(options.Value.Size);
            _darkColor = options.Value.DarkColor;
            _lightColor = options.Value.LightColor;
            _withLogo = bool.Parse(options.Value.WithLogo);
            _logoName = options.Value.LogoName;
            _eccLevel = options.Value.EccLevel switch {
                "L" => ECCLevel.L,
                "M" => ECCLevel.M,
                "Q" => ECCLevel.Q,
                _ => ECCLevel.H
            };

            _generator = new QRCodeGenerator();
        }

        public Bitmap GenerateQrCodeImage(string plainText) {
            var qrData = _generator.CreateQrCode(plainText, _eccLevel);
            var qrCode = new QRCode(qrData);

            Bitmap qrImage;
            if (_withLogo) {
                var darkColor = ColorTranslator.FromHtml(_darkColor);
                var lightColor = ColorTranslator.FromHtml(_lightColor);
                var logo = (Bitmap) Image.FromFile($"{ SharedConstants.EMAIL_TEMPLATES_DIRECTORY }{ _logoName }");

                qrImage = qrCode.GetGraphic(_size, darkColor, lightColor, logo);
            }
            else
                qrImage = qrCode.GetGraphic(_size, _darkColor, _lightColor);
            
            return qrImage;
        }
    }
}