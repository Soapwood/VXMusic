namespace VXMusic.OVRToolkit;

[Serializable]
public class OVRToolkitNotification {
    public string messageType;
    public string json;
}

[Serializable]
public class OVRToolkitNotificationBody {
    public string title;
    public string body;
    public byte[] icon;
}
