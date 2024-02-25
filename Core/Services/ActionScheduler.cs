using HarmonyLib;
using ProjectM;

namespace V.Core.Services;
[HarmonyPatch(typeof(ServerTimeSystem_Server), nameof(ServerTimeSystem_Server.OnUpdate))]
public static class ActionScheduler
{
    public static int CurrentFrameCount = 0;
    private static readonly List<ScheduledAction> scheduledActions = new List<ScheduledAction>();
    public static bool ShouldDeleteVersionFile = false;

    public static void Postfix()
    {
        CurrentFrameCount++;
        // Execute scheduled actions for the current frame
        for (int i = scheduledActions.Count - 1; i >= 0; i--)
        {
            if (scheduledActions[i].ScheduledFrame <= CurrentFrameCount)
            {
                scheduledActions[i].Execute();
                scheduledActions.RemoveAt(i);
            }
        }
    }

    public static void ScheduleAction(ScheduledAction action, int frameDelay)
    {
        action.ScheduledFrame = CurrentFrameCount + frameDelay;
        scheduledActions.Add(action);
    }
}

public class ScheduledAction
{
    public int ScheduledFrame { get; set; }
    public Delegate Callback { get; set; }
    public object[] CallbackArgs { get; set; }


    public ScheduledAction(Delegate callback, params object[] callbackArgs)
    {
        Callback = callback;
        CallbackArgs = callbackArgs;
    }

    public void Execute()
    {
        try
        {
            Callback?.DynamicInvoke(CallbackArgs);
        }
        catch (Exception e)
        {
            Unity.Debug.Log(e.ToString());
        }
    }
}

