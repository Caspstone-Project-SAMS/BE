using Base.Service.IService;

namespace Base.API.Service;

public class HangfireServiceSingleton
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public HangfireServiceSingleton(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void Run()
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var hangfireService = serviceScope.ServiceProvider.GetRequiredService<HangfireService>();

        hangfireService.CheckAbsenceRoutine();
        hangfireService.CheckDailyRoutine();
        _ = hangfireService.SlotProgress();
    }
}
