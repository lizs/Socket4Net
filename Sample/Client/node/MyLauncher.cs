using node;

namespace Sample
{
    internal class MyLauncher : Launcher<ClientConfig>
    {
        protected override void SpawnJobs()
        {
            base.SpawnJobs();
            Jobs.Create<ChatNodesMgr>(new NodesMgrArg(this, Config), false);
        }
    }
}
