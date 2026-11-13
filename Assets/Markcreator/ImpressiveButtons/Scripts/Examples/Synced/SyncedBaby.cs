
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedBaby : UdonSharpBehaviour
{
    Vector3 resetPos = new Vector3(5, 2, 0);
    Vector3 deadPos = new Vector3(-5, 2, 0);
    public Text timer;
    public GameObject wow;

    [UdonSynced] bool started = false;
    [UdonSynced] float timeLeft = 60 * 60 * 4 + 20 * 60;
    bool localStarted;
    float localTimeLeft;

    void Start()
    {
        timer.text = ToHHMMSS(localTimeLeft);
    }

    void Update()
    {
        if(localStarted)
        {
            if(localTimeLeft > 0)
            {
                localTimeLeft -= Time.deltaTime;
                timer.text = ToHHMMSS(localTimeLeft);

                gameObject.transform.localPosition += new Vector3(-2f * Time.deltaTime, 0, 0);
                if (Vector3.Distance(gameObject.transform.localPosition, deadPos) < 0.1f)
                {
                    if (Networking.LocalPlayer == null || Networking.LocalPlayer.isMaster)
                    {
                        started = localStarted = false;
                        timeLeft = localTimeLeft = 0;
                        RequestSerialization();
                    }
                }
            } else
            {
                if (!wow.activeSelf) wow.SetActive(true);
            }
        }
    }

    public override void OnDeserialization()
    {
        localTimeLeft = timeLeft;
        localStarted = started;
    }

    public void _OnPress()
    {
        if(!started)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            timeLeft = localTimeLeft = 60 * 60 * 4 + 20 * 60;
            started = localStarted = true;
            RequestSerialization();
        }
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ResetPosition");
    }

    public void ResetPosition()
    {
        gameObject.transform.localPosition = resetPos;

    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (Networking.LocalPlayer.isMaster) Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    string ToHHMMSS(float floatTime)
    {
        int time = Mathf.CeilToInt(floatTime);
        int hours = Mathf.FloorToInt(time / 3600);
        int minutes = Mathf.FloorToInt((time - (hours * 3600)) / 60);
        int seconds = time - (hours * 3600) - (minutes * 60);
        return (hours < 10 ? "0" + hours.ToString() : hours.ToString()) + ':' + (minutes < 10 ? "0" + minutes.ToString() : minutes.ToString()) + ':' + (seconds < 10 ? "0" + seconds.ToString() : seconds.ToString());
    }
}
