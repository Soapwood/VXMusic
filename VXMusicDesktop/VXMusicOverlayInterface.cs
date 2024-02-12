using System.IO.Pipes;

namespace VXMusic.Overlay;

public class VXMusicOverlayInterface
{
    public static async Task ListenForOverlayMessage(VXMusicSession recognitionClient)
    {
        using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("VXMusicOverlayEventPipe"))
        {
            Console.WriteLine("Waiting for Unity to connect...");
            await pipeServer.WaitForConnectionAsync();
            Console.WriteLine("Unity connected!");

            
            //await recognitionClient.RunRecognition();
            
            using (StreamReader reader = new StreamReader(pipeServer))
            {
                while (true)
                {
                    string eventData = reader.ReadLine();
                    if (eventData != null)
                    {
                        // Process the received event data
                        Console.WriteLine("Received event from Unity: " + eventData);
                        Perform
                        await DoThing
                    }
                }
            }
        }
    }
}