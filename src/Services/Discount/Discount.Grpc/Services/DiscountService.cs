using AutoMapper;
using Discount.Grpc.Entities;
using Discount.Grpc.Protos;
using Discount.Grpc.Repositories;
using Grpc.Core;

namespace Discount.Grpc.Services
{
    //NESTE CASO COMO E SERVER HERDAMOS DE DiscountProtoServiceClient
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly IDiscountRepository _repository;
        private readonly ILogger<DiscountService> _logger;
        private readonly IMapper _mapper;

        public DiscountService(IDiscountRepository repository, ILogger<DiscountService> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var coupon = await _repository.GetDiscount(request.ProductName);

            if (coupon is null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Discount with ProductName={request.ProductName} not found"));
            }

            _logger.LogInformation($"Discount is retrieved for ProductName: {coupon.ProductName}, Amount: {coupon.Amount} ");

            var cuoponModel = _mapper.Map<CouponModel>(coupon);
            return cuoponModel;
        }

        public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
        {
            var coupon = _mapper.Map<Coupon>(request.Coupon);

            await _repository.CreateDiscount(coupon);

            _logger.LogInformation($"Discount is successfully created. ProductName = {coupon.ProductName}");

            var couponModel = _mapper.Map<CouponModel>(coupon);

            return couponModel;
        }

        public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
        {
            var coupon = _mapper.Map<Coupon>(request.Coupon);
            await _repository.UpdateDiscount(coupon);

            _logger.LogInformation($"Discount is successfully updated. ProductName = {coupon.ProductName}");

            var couponModel = _mapper.Map<CouponModel>(coupon);
            return couponModel;
        }

        public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
        {
            await _repository.DeleteDiscount(request.Coupon.ProductName);
            var response = new DeleteDiscountResponse
            {
                Success = true
            };

            return response;




        }
    }
}