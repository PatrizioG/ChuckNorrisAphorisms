﻿using AforismiChuckNorris.Data;
using AforismiChuckNorris.Data.Entities;
using AforismiChuckNorris.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AforismiChuckNorris.Services
{
    public class AphorismsService : IAphorismsService
    {
        private readonly ILogger<AphorismsService> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AphorismsService(ILogger<AphorismsService> logger,
            ApplicationDbContext applicationDbContext,
             UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = applicationDbContext;
            _userManager = userManager;
        }

        public int GetAphorismCount() => _context.Aphorisms.Count();

        public async Task<IEnumerable<Aphorism>> GetPendingAphorism()
        {
            return await _context.Aphorisms
                .Where(a => a.Status == AphorismStatus.Pending)
                .Include(a => a.User).ToListAsync();
        }

        public async Task<Aphorism> GetAphorism(Guid aphorismId) => await _context.Aphorisms.FindAsync(aphorismId);
        public IEnumerable<Aphorism> GetAphorismsOwnedBy(string userId)
        {
            return _context.Aphorisms.Where(a => a.UserId == userId).ToList();
        }

        public async Task<bool> AddAphorism(string aphorism, string culture, string userId = null, bool saveToDb = true, AphorismStatus status = AphorismStatus.Published)
        {
            bool result = false;

            _context.Aphorisms.Add(new Aphorism()
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                Value = aphorism.Trim(),
                Culture = culture,
                UserId = userId,
                Status = status
            });

            if (saveToDb)
                result = (await _context.SaveChangesAsync()) > 0;

            _logger.LogInformation($"Added: {aphorism}");

            return result;
        }

        public async Task<Aphorism> GetRandomAphorism()
        {
            try
            {
                var ids = _context.Aphorisms.Select(a => a.Id).ToList();

                var randomIndex = new Random().Next(0, ids.Count);

                var randomId = ids.ElementAt(randomIndex);
                return await GetAphorism(randomId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return null;
        }

        public async Task<bool> SaveProduct(AphorismDto aphorismDto)
        {
            await _context.Aphorisms.AddAsync(new Aphorism()
            {
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow,
                Value = aphorismDto.Value
            });

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task UpdateProduct(Aphorism aphorism)
        {
            _context.Update(aphorism);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAphorism(Guid aphorismId)
        {
            _context.Aphorisms.Remove(new Aphorism() { Id = aphorismId });
            await _context.SaveChangesAsync();
        }
    }
}
