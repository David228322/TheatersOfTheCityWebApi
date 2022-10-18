﻿using TheatersOfTheCity.Core.Domain;

namespace TheatersOfTheCity.Core.Data;

public interface ITheaterRepository : IRepository<Theater>
{
    public Task<IEnumerable<Performance>> GetTheaterProgramsAsync(int theaterId);

}