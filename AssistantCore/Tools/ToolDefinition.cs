using System.Reflection;
using AssistantCore.Tools.Dto;

namespace AssistantCore.Tools;

public struct ToolDefinition
{
    public struct ParamData
    {
        public ParameterInfo Info;
        public LlmToolParamAttribute Attribute;
    }

    public MethodInfo Method;
    public LlmToolAttribute Attribute;
    public ParamData[] Params;

    public ToolDto ToDto()
    {
        return new ToolDto
        {
            Name = Attribute.ToolName ?? Method.Name,
            Description = Attribute.Description,
            Speciality = Attribute.Speciality,
            Parameters = Params.Select(p => new ToolParamDto
                {
                    Name = p.Attribute.ParamName,
                    Description = p.Attribute.Description,
                    Required = p.Attribute.Required,
                    Type = p.Info.ParameterType is { IsPrimitive: true, IsEnum: false }
                        ? p.Info.ParameterType.ToString().ToLower() 
                        : "string",
                    Enum = p.Info.ParameterType.IsEnum
                        ? p.Info.ParameterType.GetEnumNames()
                        : null
                })
                .ToArray()
        };
    }
}