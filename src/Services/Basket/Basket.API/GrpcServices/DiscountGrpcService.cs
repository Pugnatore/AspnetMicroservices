using System.Formats.Asn1;
using Discount.Grpc.Protos;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcService
    {
        //NESTE CASO COMO E CLIENTE NAO HERDAMOS DE DiscountProtoServiceClient
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;

        public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoService)
        {
            _discountProtoService = discountProtoService;
        }

        public async Task<CouponModel> GetDiscount(string productName)
        {
            var discountRequest = new GetDiscountRequest
            {
                ProductName = productName
            };

            return await _discountProtoService.GetDiscountAsync(discountRequest);
        } 
    }
}
