using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IUnitOfWork _uow;

        public PaymentMethodService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<PaymentMethod>> GetAllAsync(CancellationToken ct = default)
        {
            return await _uow.PaymentMethodRepository.GetAllQueryable()
                .AsNoTracking()
                .OrderBy(pm => pm.Name)
                .ToListAsync(ct);
        }

        public async Task<List<PaymentMethod>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _uow.PaymentMethodRepository.GetAllQueryable()
                .AsNoTracking()
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.Name)
                .ToListAsync(ct);
        }

        public async Task<bool> SetStatusAsync(int id, bool isActive, string modifiedBy, CancellationToken ct = default)
        {
            var method = await _uow.PaymentMethodRepository.GetByIdAsync(id);
            if (method == null)
            {
                return false;
            }

            method.IsActive = isActive;
            method.ModifiedBy = string.IsNullOrWhiteSpace(modifiedBy) ? "system" : modifiedBy.Trim();
            method.ModifiedAt = DateTime.UtcNow;

            _uow.PaymentMethodRepository.Update(method);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
