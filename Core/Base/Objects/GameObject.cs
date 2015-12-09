using System.Collections.Generic;
using System.Linq;
using Pi.Core.Common;
using Pi.Core.Protocol;
using Pi.socket4net.RPC;

namespace Pi.Core
{
    public abstract partial class GameObject : UniqueObj<long>, IGameObject
    {
        public EGameObjectType Type { get; protected set; }

        public T AddRpcComponent<T>() where T : class, IRpcComponent, new()
        {
            return Components.Create<T>(EComponentId.Invalid, this, Session);
        }

        public IRpcComponent GetRpcComponent(EComponentId id)
        {
            IComponent ret = Components.Get(id);
            return ret != null ? ret as IRpcComponent : null;
        }

        #region ISerializable<HostedBlock>

        public List<BlockProto> Serialize()
        {
            return Blocks.Where(block => block.Serializable)
                .Select(x => new BlockProto {PropertyId = x.Id, Data = x.Serialize()}).ToList();
        }

        public bool Deserialize(List<BlockProto> protos)
        {
            protos.ForEach(proto =>
            {
                PropertyBody.InjectIfNotExist(proto.PropertyId);
                IBlock block = PropertyBody.GetBlock(proto.PropertyId);
                block.Deserialize(proto.Data);
                NotifyPropertyChanged(block);
            });
            return true;
        }

        #endregion

        #region properties

        public int Level
        {
            get { return Get<int>(EPropertyId.Level); }
            set { IncTo(EPropertyId.Level, value); }
        }

        public int ConfigId { get; private set; }

        public List<string> GetFeilds()
        {
            return PropertyBody.Where(x => x.Serializable).Select(x => x.RedisFeild).ToList();
        }

        #endregion

        #region initialization

        public virtual void Init(long id, IObj host, int cfgId, IEnumerable<KeyValuePair<EPropertyId, byte[]>> values,
            IRpcSession session, bool reset)
        {
            ConfigId = cfgId;
            Type = GameObjectMapping.ToGoType(ConfigId);
            Session = session;
            Init(id, host, values, reset);
        }
        
        public virtual void Boot()
        {
            foreach (var component in Components)
                component.Boot();
        }
        #endregion

        protected virtual void OnPropertyChanged(IBlock block)
        {
            foreach (var component in Components)
            {
                component.OnPropertyChanged(block);
            }
        }
        
        #region rpc

        public IRpcSession Session { get; private set; }

        public virtual RpcResult HandleRequest(uint ops, byte[] param)
        {
            return RpcResult.Failure;
        }

        public virtual bool HandlePush(uint ops, byte[] param)
        {
            return false;
        }

        #endregion

        private string _redisFeild;
        public string RedisFeild
        {
            get { return _redisFeild ?? (_redisFeild = string.Format("{0}:{1}", ConfigId, Id)); }
        }

        public virtual void Reset()
        {
            IncTo(EPropertyId.Level, 1);

            foreach (var component in Components)
            {
                component.Reset();
            }
        }

        /// <summary>
        /// 附带自定义数据
        /// </summary>
        protected object UserData { get; set; }
        public T GetUserData<T>() where T : class
        {
            return UserData != null ? UserData as T : null;
        }

        public void SetUserData(object obj)
        {
            UserData = obj;
        }
    }
}