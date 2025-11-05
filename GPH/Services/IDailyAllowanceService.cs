public interface IDailyAllowanceService {
    Task<bool> CheckAndAwardExecutiveDA(int executiveId, DateTime date);
    Task CheckAndAwardAsmDA(int executiveId, DateTime date);
}