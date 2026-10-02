namespace Keshiya
{
    // Session-only. Separate revenue ledgers and a spending boundary for a future shop.
    [System.Serializable] public sealed class Wallet
    {
        [field:UnityEngine.SerializeField] public long Balance {get;private set;}
        [field:UnityEngine.SerializeField] public long JobIncome {get;private set;}
        [field:UnityEngine.SerializeField] public long CrumbIncome {get;private set;}
        [UnityEngine.SerializeField] long lastPaidJob;
  public long ShopSpent,DebugIncome;
  public void DebugCredit(long amount){if(amount<=0||amount>1000000)return;Balance+=amount;DebugIncome+=amount;}
        public bool CreditJob(long jobId,int amount)
        {
            if(jobId<=lastPaidJob||amount<0)return false;
            lastPaidJob=jobId;JobIncome+=amount;Balance+=amount;return true;
        }
        internal void CreditCrumbs(long amount){if(amount<=0)return;CrumbIncome+=amount;Balance+=amount;}
        public bool TrySpend(long amount){if(amount<=0||amount>Balance)return false;Balance-=amount;ShopSpent+=amount;return true;}
    }
}
