namespace socket4net
{
    public class FuzzyObjArg<TKey> : UniqueObjArg<TKey>
    {
        public FuzzyObjArg(Obj parent, TKey key, params object[] param) : base(parent, key)
        {
            if (param.Length <= 0) return;

            Param = new object[param.Length];
            for (var i = 0; i < param.Length; i++)
            {
                Param[i] = param[i];
            }
        }

        public object[] Param { get; private set; }
    }

    /// <summary>
    ///     拥有不定长初始参数的对象
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class FuzzyObj<TKey> : UniqueObj<TKey>
    {
        //protected override void OnInit()
        //{
        //    base.OnInit();
        //    var more = Argument as FuzzyObjArg<TKey>;

        //}
    }
}