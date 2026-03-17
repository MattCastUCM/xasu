using System;
using System.Threading.Tasks;
using Xasu;
using Xasu.HighLevel;

public static class TestTrackerCalls
{
    public static async Task Init()
    {
        await XasuTracker.Init();
        XasuTracker.Log("Initializing TrackerManager");
    }
    public static async Task Quit()
    {
        // Si el tracker no se ha inicializado o si ha finalizado, no hace nada
        if (XasuTracker.Status.State == TrackerState.Uninitialized || XasuTracker.Status.State == TrackerState.Finalized)
        {
            return;
        }
        XasuTracker.Log("Quitting TrackerManager");

        Progress<float> progress = new Progress<float>();
        progress.ProgressChanged += (_, p) =>
        {
            XasuTracker.Log("Finalization progress: " + p);
        };
        await XasuTracker.Finalize(progress);
        XasuTracker.Log("Tracker finalized");
    }


    public static async Task TrySendStatement(StatementPromise promise)
    {
        // Si el tracker esta en estado normal, se intenta enviar la traza
        if (XasuTracker.Status.State == TrackerState.Normal)
        {
            try
            {
                var statement = await promise.Promise;
                XasuTracker.Log("Completed statement sent with id: " + promise.Statement.id);
            }
            catch (AggregateException aggEx)
            {
                XasuTracker.Log("Failed! " + aggEx.GetType().ToString());
                foreach (var ex in aggEx.InnerExceptions)
                {
                    XasuTracker.Log("Inner Exception: " + ex.GetType().ToString());
                }
            }
        }
    }
}