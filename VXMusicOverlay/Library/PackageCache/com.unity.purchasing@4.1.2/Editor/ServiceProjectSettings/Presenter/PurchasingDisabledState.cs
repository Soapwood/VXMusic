namespace UnityEditor.Purchasing
{
    internal class PurchasingDisabledState : BasePurchasingState
    {
        internal const string k_StateNameDisabled = "DisabledState";

        public PurchasingDisabledState(SimpleStateMachine<bool> stateMachine)
            : base(k_StateNameDisabled, stateMachine)
        {
            ModifyActionForEvent(true, HandleEnabling);
        }

        SimpleStateMachine<bool>.State HandleEnabling(bool raisedEvent)
        {
            return stateMachine.GetStateByName(PurchasingEnabledState.k_StateNameEnabled);
        }

        protected override AnalyticsNoticeBlock CreateAnalyticsNoticeBlock()
        {
            return AnalyticsNoticeBlock.CreateDisabledAnalyticsBlock();
        }

        internal override bool IsEnabled() => false;
    }
}
