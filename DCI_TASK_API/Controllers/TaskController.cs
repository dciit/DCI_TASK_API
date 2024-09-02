using DCI_TASK_API.Contexts;
using DCI_TASK_API.Interfaces;
using DCI_TASK_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace DCI_TASK_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly DBSCM efSCM;
        private readonly DBHRM efHRM;
        public readonly List<string> Steps = new List<string>() { "doing", "done", "reject" };
        public TaskController(DBSCM efSCM, DBHRM efHRM)
        {
            this.efSCM = efSCM;
            this.efHRM = efHRM;
        }

        [HttpGet]
        [Route("/GetStatistic/{empcode}")]
        public IActionResult GetStatistic(string empcode)
        {
            List<DciTask> taskList = efSCM.DciTasks.Where(x => x.TaskCreateBy == empcode).OrderByDescending(x => x.TaskUpdateDt).ToList();
            return Ok(new
            {
                total = taskList.Count,
                doing = taskList.Where(x => x.TaskStatus == "doing").Count(),
                done = taskList.Where(x => x.TaskStatus == "done").Count(),
                reject = taskList.Where(x => x.TaskStatus == "reject").Count()
            });
        }

        [HttpPost]
        [Route("/GetTasks")]
        public IActionResult GetTasks([FromBody] ParamGetTasks param)
        {
            DateTime dtST = DateTime.ParseExact($"{param.dtST} 00:00:01", "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime dtFN = DateTime.ParseExact($"{param.dtFN} 23:59:59", "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture);
            string taskStatus = param.taskStatus;
            string taskPriority = param.taskPriority;
            string taskCreateBy = param.taskCreateBy;
            List<DciTask> tasks = efSCM.DciTasks.Where(x => x.TaskCreateDt >= dtST && x.TaskCreateDt <= dtFN && (taskStatus != "" ? x.TaskStatus == taskStatus : true) && (taskPriority != "" ? x.TaskPriority == taskPriority : true) && (taskCreateBy != "" ? x.TaskCreateBy == taskCreateBy : true) && x.TaskStatus != "delete").OrderByDescending(x => x.TaskUpdateDt).ToList();
            return Ok(tasks);
        }

        [HttpGet]
        [Route("/GetTaskById/{taskId}")]
        public IActionResult GetTaskByID(int taskId)
        {
            DciTask task = efSCM.DciTasks.FirstOrDefault(x => x.TaskId == taskId)!;
            return Ok(task);
        }

        [HttpGet]
        [Route("/RejectTask/{taskId}/{empcode}")]
        public IActionResult RejectTask(int taskId, string empcode)
        {
            try
            {
                DciTask oTask = efSCM.DciTasks.FirstOrDefault(x => x.TaskId == taskId)!;
                if (oTask != null)
                {
                    oTask.TaskStatus = "reject";
                    oTask.TaskUpdateDt = DateTime.Now;
                    oTask.TaskUpdateBy = empcode;
                    efSCM.Update(oTask);
                    int reject = efSCM.SaveChanges();
                    return Ok(new PropStatus() { status = reject > 0 ? true : false, message = "เกิดข้อผิดพลาดระหว่างยกเลิกงาน" });
                }
                else
                {
                    return Ok(new PropStatus() { status = false, message = "ไม่พบข้อมูลงานของคุณ" });
                }
            }
            catch (Exception e)
            {
                return Ok(new PropStatus()
                {
                    status = false,
                    message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("/DeleteTask/{taskId}/{code}")]
        public IActionResult DeleteTask(int taskId, string code)
        {
            try
            {
                DciTask oTask = efSCM.DciTasks.FirstOrDefault(x => x.TaskId == taskId);
                if (oTask != null)
                {
                    oTask.TaskStatus = "delete";
                    oTask.TaskUpdateBy = code;
                    oTask.TaskUpdateDt = DateTime.Now;
                    efSCM.DciTasks.Update(oTask);
                    int delete = efSCM.SaveChanges();
                    return Ok(new PropStatus() { status = delete > 0 ? true : false, message = "เกิดข้อผิดพลาดระหว่างลบข้อมูล" });
                }
                else
                {
                    return Ok(new PropStatus()
                    {
                        status = false,
                        message = "ไม่พบข้อมูล"
                    });
                }
            }
            catch (Exception e)
            {
                return Ok(new PropStatus()
                {
                    status = false,
                    message = e.Message
                });
            }
        }

        [HttpPost]
        [Route("/InsertTask")]
        public IActionResult InsertTask([FromBody] DciTask task)
        {
            try
            {
                task.TaskStatus = "doing";
                task.TaskCreateDt = DateTime.Now;
                task.TaskUpdateDt = DateTime.Now;
                efSCM.DciTasks.Add(task);
                int insert = efSCM.SaveChanges();
                return Ok(new PropStatus()
                {
                    status = insert > 0 ? true : false,
                    message = ""
                });
            }
            catch (Exception e)
            {
                return Ok(new PropStatus()
                {
                    status = false,
                    message = e.Message
                });
            }
        }
        [HttpPost]
        [Route("/UpdateTaskStatus")]
        public IActionResult UpdateTaskStatus([FromBody] ParamUpdateTaskStatus param)
        {
            try
            {
                int taskId = param.taskId;
                bool taskAction = param.taskAction;
                string taskUpdateBy = param.taskUpdateBy;
                if (taskId != 0)
                {
                    DciTask oTask = efSCM.DciTasks.FirstOrDefault(x => x.TaskId == taskId)!;
                    if (oTask != null)
                    {
                        string taskStatus = oTask.TaskStatus;
                        int taskPrevIndex = Steps.FindIndex(x => x == taskStatus);
                        if (taskAction == true)
                        {
                            taskPrevIndex++;
                            if (taskPrevIndex > Steps.Count)
                            {
                                taskPrevIndex = Steps.Count - 1;
                            }
                            taskStatus = Steps[taskPrevIndex];
                        }
                        else
                        {
                            taskPrevIndex--;
                            if (taskPrevIndex < 0)
                            {
                                taskPrevIndex = 0;
                            }
                            taskStatus = Steps[0];
                        }
                        oTask.TaskStatus = taskStatus;
                        oTask.TaskUpdateBy = taskUpdateBy;
                        oTask.TaskUpdateDt = DateTime.Now;
                        efSCM.DciTasks.Update(oTask);
                        int update = efSCM.SaveChanges();
                        return Ok(new PropStatus() { status = update > 0 ? true : false, message = "เกิดข้อผิดพลาดระหว่างแก้ไขข้อมูล" });
                    }
                    else
                    {
                        return Ok(new PropStatus() { status = false, message = "ไม่พบข้อมูลงานของคุณ" });
                    }
                }
                else
                {
                    return Ok(new PropStatus() { status = false, message = "ไม่พบข้อมูลสถานะงานของคุณ" });
                }
            }
            catch (Exception e)
            {
                return Ok(new PropStatus()
                {
                    status = false,
                    message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("/LoginByCode/{code}")]
        public IActionResult LoginByCode(string code)
        {
            return Ok();
        }

        [HttpPost]
        [Route("/EditTask")]
        public IActionResult EditTask([FromBody] DciTask param)
        {
            try
            {
                int taskId = param.TaskId;
                DciTask oTask = efSCM.DciTasks.FirstOrDefault(x => x.TaskId == taskId);
                if (oTask != null)
                {
                    oTask.TaskTitle = oTask.TaskTitle != param.TaskTitle ? param.TaskTitle : oTask.TaskTitle;
                    oTask.TaskDesc = oTask.TaskDesc != param.TaskDesc ? param.TaskDesc : oTask.TaskDesc;
                    oTask.TaskWarning = oTask.TaskWarning != param.TaskWarning ? param.TaskWarning : oTask.TaskWarning;
                    oTask.TaskPriority = oTask.TaskPriority != param.TaskPriority ? param.TaskPriority : oTask.TaskPriority;
                    oTask.TaskDuedate = oTask.TaskDuedate != param.TaskDuedate ? param.TaskDuedate : oTask.TaskDuedate;
                    oTask.TaskUpdateBy = param.TaskUpdateBy;
                    oTask.TaskUpdateDt = DateTime.Now;
                    efSCM.DciTasks.Update(oTask);
                    int update = efSCM.SaveChanges();
                    return Ok(new
                    {
                        status = update > 0 ? true : false,
                        message = update == 0 ? "เกิดข้อผิดพลาดระหว่างแก้ไขข้อมูล" : ""
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        message = "ไม่พบข้อมูล"
                    });
                }
            }
            catch (Exception e)
            {
                return Ok(new
                {
                    status = false,
                    message = e.Message
                });
            }
        }

        [HttpGet]
        [Route("/GetCreateBys")]
        public IActionResult GetCreateBys()
        {
            List<PropCreateBys> CreateBys = efHRM.Employees.Where(x => (x.Dvcd == "11210" || x.Dvcd == "11200") && (!x.Resign.HasValue || x.Resign.Value == new DateTime(1900, 1, 1))).Select(x => new PropCreateBys() { code = x.Code, name = x.Name!, surn = x.Surn!, fullName = $"{x.Name}.{x.Surn.Substring(0, 1)}" }).OrderBy(x => x.code).ToList();
            return Ok(CreateBys);
        }
    }
}