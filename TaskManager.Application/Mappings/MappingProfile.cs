using AutoMapper;
using TaskManager.Application.DTOs.TaskDtos;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region Entrada: DTO -> Entidade
            CreateMap<TaskResponseDto, TaskItem>();
            #endregion

            #region Saída: Entidade -> DTO
            CreateMap<TaskItem, TaskResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatus(src.Status)));
            #endregion
        }

        private string MapStatus(TaskItemStatus status) => status switch
        {
            TaskItemStatus.Pending => "Pendente",
            TaskItemStatus.InProgress => "Em progresso",
            TaskItemStatus.Completed => "Concluída",
            _ => status.ToString()
        };
    }
}
