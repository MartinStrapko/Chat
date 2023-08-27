using ChatApp;
using ChatApp.Interfaces;
using ChatApp.Models;
using ChatApp.Services;
using System.Xml.Linq;


var morningShift = new Shift { Name = "Morning", Start = TimeSpan.FromHours(8), End = TimeSpan.FromHours(16) };
var afternoonShift = new Shift { Name = "Afternoon", Start = TimeSpan.FromHours(16), End = TimeSpan.FromHours(24) };
var nightShift = new Shift { Name = "Night", Start = TimeSpan.FromHours(0), End = TimeSpan.FromHours(8) };
var allDay = new Shift { Name = "All Day", Start = TimeSpan.FromHours(0), End = TimeSpan.FromHours(8) };

var teamA = new Team { Name = "Team A", Shift = morningShift, Agents = new List<Agent>() };
var teamB = new Team { Name = "Team B", Shift = afternoonShift, Agents = new List<Agent>() };
var teamC = new Team { Name = "Team C", Shift = nightShift, Agents = new List<Agent>() };
var overflowTeam = new Team { Name = "Overflow Team", Shift = allDay, Agents = new List<Agent>() };

teamA.Agents.Add(new Agent { Name = "Team Lead 1", Seniority = ChatApp.Enums.AgentSeniority.TeamLead });
teamA.Agents.Add(new Agent { Name = "Mid Level 1", Seniority = ChatApp.Enums.AgentSeniority.MidLevel });
teamA.Agents.Add(new Agent { Name = "Mid Level 2", Seniority = ChatApp.Enums.AgentSeniority.MidLevel });
teamA.Agents.Add(new Agent { Name = "Junior 1", Seniority = ChatApp.Enums.AgentSeniority.Junior });

teamB.Agents.Add(new Agent { Name = "Senior 1", Seniority = ChatApp.Enums.AgentSeniority.Senior });
teamB.Agents.Add(new Agent { Name = "Junior 1", Seniority = ChatApp.Enums.AgentSeniority.Junior });
teamB.Agents.Add(new Agent { Name = "Junior 2", Seniority = ChatApp.Enums.AgentSeniority.Junior });
teamB.Agents.Add(new Agent { Name = "Mid Level 1", Seniority = ChatApp.Enums.AgentSeniority.MidLevel });

teamC.Agents.Add(new Agent { Name = "Mid Level 1", Seniority = ChatApp.Enums.AgentSeniority.MidLevel });
teamC.Agents.Add(new Agent { Name = "Mid Level 2", Seniority = ChatApp.Enums.AgentSeniority.MidLevel });

for (int i = 0; i < 10; i++)
{
    overflowTeam.Agents.Add(new Agent{ Name = "Junior " + i, Seniority = ChatApp.Enums.AgentSeniority.Junior });
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<IQueueService, QueueService>();
builder.Services.AddSingleton<IAgentService, AgentService>();

var teams = new List<Team> { teamA, teamB, teamC, overflowTeam };
var shifts = new List<Shift> { morningShift, afternoonShift, nightShift, allDay };
builder.Services.AddSingleton(teams);
builder.Services.AddSingleton(shifts);
builder.Services.AddHostedService<CheckMissedPollsService>();
builder.Services.Configure<ChatSettings>(builder.Configuration.GetSection("ChatSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "Hello World!");

app.Run();
