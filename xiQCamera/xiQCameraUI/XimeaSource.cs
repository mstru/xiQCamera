using System.Threading;

namespace xiQCameraUI
{
    internal class XimeaSource
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private readonly MainWindow UI;

        /// <summary>
        /// Objekt na synchronizaciu
        /// </summary>
        private object sync = new object();

        public XimeaSource(MainWindow main)
        {
            UI = main;
            cts = new CancellationTokenSource();
        }
    }
}
