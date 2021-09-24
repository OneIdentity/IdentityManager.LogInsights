using System;


namespace LogInsights.Detectors
{
    public interface ILogDetector
    {
        void InitializeDetector();

        void FinalizeDetector();
        void ProcessMessage(TextMessage msg);

        bool isEnabled { get; set; }

        string identifier { get; }

        string caption { get; }

        Datastore.DataStore datastore { set; }

        ILogDetector[] parentDetectors { set; }

        string[] requiredParentDetectors { get;  }
    }
   
}
