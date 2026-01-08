namespace AgentPay.Application.DTOs;

public class BlockchainStatusDto
{
    public long LatestBlock { get; set; }
    public decimal CurrentGasPrice { get; set; }
    public int PeerCount { get; set; }
    public bool IsSyncing { get; set; }
    public string NetworkName { get; set; }
}
