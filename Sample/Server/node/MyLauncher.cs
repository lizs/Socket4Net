using node;

namespace Sample
{
    internal class MyLauncher : Launcher<ServerConfig>
    {
        protected override void SpawnJobs()
        {
            base.SpawnJobs();
            Jobs.Create<ServerNodesMgr>(new NodesMgrArg(this, Config), false);
        }
    }
}
