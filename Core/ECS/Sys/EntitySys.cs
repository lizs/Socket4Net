using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace socket4net
{
    /// <summary>
    ///     实体管理器参数
    ///     包括：
    ///     1、 属性块创建器
    ///     2、 属性Feild格式化器
    ///     3、 属性Feild萃取器
    /// </summary>
    public class EntitySysArg : UniqueMgrArg
    {
        public Func<short, IBlock> BlockMaker { get; private set; }
        public Func<long, short, string> BlockFeildFormatter { get; private set; }
        public Func<string, short> BlockFeildExtractor { get; private set; }

        public EntitySysArg(IUniqueObj<long> parent, Func<short, IBlock> blockMaker, Func<long, short, string> blockFeildFormatter,
            Func<string, short> blockFeildExtractor) : base(parent)
        {
            BlockMaker = blockMaker;
            BlockFeildFormatter = blockFeildFormatter;
            BlockFeildExtractor = blockFeildExtractor;
        }
    }

    /// <summary>
    ///     实体管理器
    ///     包括：
    ///     1、 属性块创建器
    ///     2、 属性Feild格式化器
    ///     3、 属性Feild萃取器
    /// 
    ///     4、 属性发布器
    /// 
    ///     5、 实体同步
    ///     6、 实体存取
    /// </summary>
    public sealed partial class EntitySys : UniqueMgr<long, Entity>
    {
        private Func<short, IBlock> _blockMaker;
        private Func<long, short, string> _blockFeildFormatter;
        private Func<string, short> _blockFeildExtractor;

        //public event Action<Entity> EventEntityExtracted;
        //public event Action<Entity> EventEntityCreated;

        private string _key;
        public string Key
        {
            get
            {
                if (!_key.IsNullOrEmpty()) return _key;
                var owner = GetAncestor<IUniqueObj<long>>();
                _key = FormatFeild(owner.Id, owner.GetType());

                return _key;
            }
        }

        protected override void OnInit(ObjArg arg)
        {
            base.OnInit(arg);
            var more = arg.As<EntitySysArg>();

            _blockMaker = more.BlockMaker;
            _blockFeildFormatter = more.BlockFeildFormatter;
            _blockFeildExtractor = more.BlockFeildExtractor;

            if (_blockMaker == null || _blockFeildFormatter == null || _blockFeildExtractor == null)
                throw new ArgumentException("arg");
        }

        #region 提取

        public T Extract<T>(EntityEntry entry) where T : Entity, new()
        {
            var entity = Create<T>(entry.Type, new EntityArg(this, entry.Id));
            if (entity == null) return null;

            if (entry.Blocks.IsNullOrEmpty()) return entity;
            var blocks = entry.Blocks.Select(ExtractBlock).Where(block => block != null).ToArray();
            entity.Apply(blocks);

            return entity;
        }

        public IEnumerable<T> Extract<T>(IReadOnlyCollection<EntityEntry> entries)
            where T : Entity, new()
        {
            return entries.IsNullOrEmpty() ? null : entries.Select(Extract<T>);
        }

        #endregion

        #region 输出

        /// <summary>
        ///     格式化输出一个实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IReadOnlyCollection<RedisEntry> FormatEntity(Entity entity)
        {
            return entity.Blocks.Select(x=>FormatBlock(entity.Id, x)).ToArray();
        }

        /// <summary>
        ///     格式化输出一个属性块
        /// </summary>
        /// <param name="id"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public RedisEntry FormatBlock(long id, IBlock block)
        {
            return new RedisEntry { Feild = _blockFeildFormatter(id, block.Id), Data = block.Serialize() };
        }

        #endregion


        #region helper

        public string FormatFeild(long id, Type type)
        {
            return string.Format("{0}:{1}", type.Name, id);
        }

        //public EntityEntry ExtractFeild(string feild)
        //{
        //    var fileds = feild.Split(':');
        //    return new EntityEntry { Type = Type.GetType(fileds[0]), Id = long.Parse(fileds[1]) };
        //}

        private IBlock ExtractBlock(RedisEntry entry)
        {
            var pid = _blockFeildExtractor(entry.Feild);
            var block = _blockMaker(pid);
            return block.Deserialize(entry.Data) ? block : null;
        }

        private bool ExtractBlock(RedisEntry entry, ref IBlock item)
        {
            return item.Deserialize(entry.Data);
        }

        private RedisEntry SerializeBlock(long id, IBlock block)
        {
            var feild = _blockFeildFormatter(id, block.Id);
            var data = block is IDifferentialSerializable
                ? (block as IDifferentialSerializable).SerializeDifference()
                : block.Serialize();

            return new RedisEntry { Feild = feild, Data = data };
        }
        #endregion
    }
}