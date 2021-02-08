using System.Drawing;

namespace AssistantLibrary.Interfaces {

    public interface IQrCodeMaker {

        Bitmap GenerateQrCodeImage(string plainText);
    }
}