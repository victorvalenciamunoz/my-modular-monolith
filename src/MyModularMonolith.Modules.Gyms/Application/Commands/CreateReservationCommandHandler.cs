using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyModularMonolith.Modules.Gyms.Application.Mappers;
using MyModularMonolith.Modules.Gyms.Contracts;
using MyModularMonolith.Modules.Gyms.Contracts.Commands;
using MyModularMonolith.Modules.Gyms.Domain;
using MyModularMonolith.Modules.Gyms.Domain.Specifications;
using MyModularMonolith.Shared.Application;
using MyModularMonolith.Shared.Domain;
using static MyModularMonolith.Modules.Gyms.Domain.Specifications.GetReservationsByGymSpec;

namespace MyModularMonolith.Modules.Gyms.Application.Commands;

internal class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ErrorOr<ReservationDto>>
{
    private readonly IGymProductRepository _gymProductRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateReservationCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateReservationCommandHandler(
        IGymProductRepository gymProductRepository,
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<CreateReservationCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _gymProductRepository = gymProductRepository;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ErrorOr<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var userReservationForSlotSpec = new UserReservationForSlotSpec(
        request.UserId,
        request.GymProductId,
        request.ReservationDateTime);

        var existingReservation = await _reservationRepository.FirstOrDefaultAsync(userReservationForSlotSpec, cancellationToken);
        if (existingReservation != null)
        {
            return Error.Conflict("Reservation.DuplicateSlotReservation",
                "User already has a reservation for this product and time slot");
        }

        var gymProductSpec = new GetGymProductByIdWithNavigationSpec(request.GymProductId);
        var gymProduct = await _gymProductRepository.FirstOrDefaultAsync(gymProductSpec, cancellationToken);
        if (gymProduct == null)
        {
            return Error.NotFound("GymProduct.NotFound", $"Gym product with ID {request.GymProductId} was not found");
        }

        var userMembershipLevel = GetUserMembershipLevel();
        if (!gymProduct.Product.IsAccessibleForMembership(userMembershipLevel))
        {
            _logger.LogWarning("User {UserId} with membership level {UserMembership} attempted to access product {ProductId} requiring {RequiredMembership}",
                request.UserId, userMembershipLevel, gymProduct.ProductId, gymProduct.Product.MinimumRequiredMembership);

            return Error.Forbidden("Product.InsufficientMembership",
                $"This product requires {gymProduct.Product.MinimumRequiredMembership} membership level. Your current level is {userMembershipLevel}.");
        }

        var slotReservationsSpec = new ReservationsBySlotSpec(request.GymProductId, request.ReservationDateTime);
        var currentReservations = await _reservationRepository.ListAsync(slotReservationsSpec, cancellationToken);
        var currentReservationCount = currentReservations.Count(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed);

        try
        {            
            var reservation = new Reservation(
                request.UserId,
                request.GymProductId,
                request.ReservationDateTime,
                request.UserNotes,
                _dateTimeProvider.UtcNow);
                        
            reservation.ValidateBusinessRules(gymProduct, currentReservationCount);

            await _reservationRepository.AddAsync(reservation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ReservationMapper.ToDto(reservation, gymProduct);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Validation("Reservation.ValidationError", ex.Message);
        }
    }

    private MembershipLevel GetUserMembershipLevel()
    {
        // Default to Standard if we can't determine the membership level
        MembershipLevel membershipLevel = MembershipLevel.Standard;

        // Get the membership level from the authenticated user's claims
        var user = _httpContextAccessor?.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var membershipClaim = user.Claims.FirstOrDefault(c => c.Type == "membershipLevel");
            if (membershipClaim != null && int.TryParse(membershipClaim.Value, out int membershipValue))
            {
                // Ensure the value is valid for the enum
                if (Enum.IsDefined(typeof(MembershipLevel), membershipValue))
                {
                    membershipLevel = (MembershipLevel)membershipValue;
                }
            }
        }

        return membershipLevel;
    }
}