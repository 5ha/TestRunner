﻿using ComposeManager.Services;
using JobModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ComposeManager.Controllers
{
    public class ManagerController : ControllerBase
    {
        private readonly IJobRunner _jobRunner;

        public ManagerController(IJobRunner jobRunner)
        {
            _jobRunner = jobRunner;
        }

        [HttpGet("/health")]
        public ActionResult<string> Index()
        {
            return "SUCCESS";
        }

        // Start a job
        [HttpPost("/start")]
        public async Task<ActionResult<string>> StartJob([FromBody]JobDescription jobDescription)
        {
            string project = Guid.NewGuid().ToString("N");

            await _jobRunner.RunJob(jobDescription, $"{project}");

            return $"STARTED: {jobDescription.Build}";
        }
    }
}
