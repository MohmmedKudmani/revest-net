namespace ProductService.DTOs;

public record WriteResponseDto<T>(string Message, T Data);
