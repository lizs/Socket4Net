namespace socket4net
{
    public abstract class Command : Obj
    {
        public abstract void Excute();
    }

    public abstract class RpcCommand : Obj
    {
        public abstract RpcResult Excute();
    }
}
