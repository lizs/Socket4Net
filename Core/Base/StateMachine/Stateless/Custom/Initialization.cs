
namespace Stateless
{
    /// <summary>
    /// 增加初始接口
    /// 在初始接口中重新初始状态
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    public partial class StateMachine<TState, TTrigger>
    {
        public StateMachine()
        {
            var reference = new StateReference { State = default(TState) };
            _stateAccessor = () => reference.State;
            _stateMutator = s => reference.State = s;
        }

        public void Init(TState initialState)
        {
            State = initialState;
            CurrentRepresentation.Enter(new Transition(default(TState), initialState, default(TTrigger)));
        }
    }
}
