﻿// -----------------------------------------------------------------------
// <copyright file="EventTask.cs" company="Nodine Legal, LLC">
// Licensed to Nodine Legal, LLC under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  Nodine Legal, LLC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenLawOffice.Data.Events
{
    using System;
    using System.Data;
    using System.Linq;
    using AutoMapper;
    using Dapper;
    using System.Collections.Generic;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class EventTask
    {
        public static Common.Models.Events.EventTask Get(Guid id)
        {
            return DataHelper.Get<Common.Models.Events.EventTask, DBOs.Events.EventTask>(
                "SELECT * FROM \"event_task\" WHERE \"id\"=@id AND \"utc_disabled\" is null",
                new { id = id });
        }

        public static Common.Models.Events.EventTask Get(long taskId, Guid eventId)
        {
            return DataHelper.Get<Common.Models.Events.EventTask, DBOs.Events.EventTask>(
                "SELECT * FROM \"event_task\" WHERE \"task_id\"=@TaskId AND \"event_id\"=@EventId AND \"utc_disabled\" is null",
                new { TaskId = taskId, EventId = eventId });
        }

        public static Common.Models.Events.EventTask GetFor(long taskId)
        {
            return DataHelper.Get<Common.Models.Events.EventTask, DBOs.Events.EventTask>(
                "SELECT * FROM \"event_task\" WHERE \"task_id\"=@TaskId AND \"utc_disabled\" is null",
                new { TaskId = taskId });
        }

        public static Common.Models.Events.EventTask GetFor(Guid eventId)
        {
            return DataHelper.Get<Common.Models.Events.EventTask, DBOs.Events.EventTask>(
                "SELECT * FROM \"event_task\" WHERE \"event_id\"=@EventId AND \"utc_disabled\" is null",
                new { EventId = eventId });
        }

        public static List<Common.Models.Tasks.Task> ListForEvent(Guid eventId)
        {
            List<Common.Models.Tasks.Task> list =
                DataHelper.List<Common.Models.Tasks.Task, DBOs.Tasks.Task>(
                "SELECT * FROM \"task\" WHERE \"id\" IN (SELECT \"task_id\" FROM \"event_task\" WHERE \"event_id\"=@EventId AND \"utc_disabled\" is null)",
                new { EventId = eventId });

            return list;
        }

        public static Common.Models.Events.EventTask Create(Common.Models.Events.EventTask model,
            Common.Models.Account.Users creator)
        {
            DBOs.Events.EventTask dbo;
            Common.Models.Events.EventTask currentModel;

            if (!model.Id.HasValue) model.Id = Guid.NewGuid();
            model.Created = model.Modified = DateTime.UtcNow;
            model.CreatedBy = model.ModifiedBy = creator;

            currentModel = Get(model.Task.Id.Value, model.Event.Id.Value);

            if (currentModel != null)
                return currentModel;

            dbo = Mapper.Map<DBOs.Events.EventTask>(model);

            using (IDbConnection conn = Database.Instance.GetConnection())
            {
                conn.Execute("INSERT INTO \"event_task\" (\"id\", \"task_id\", \"event_id\", \"utc_created\", \"utc_modified\", \"created_by_user_pid\", \"modified_by_user_pid\") " +
                    "VALUES (@Id, @TaskId, @EventId, @UtcCreated, @UtcModified, @CreatedByUserPId, @ModifiedByUserPId)",
                    dbo);
            }

            return model;
        }

        public static void Delete(Common.Models.Events.EventTask model, Common.Models.Account.Users deleter)
        {
            using (IDbConnection conn = Database.Instance.GetConnection())
            {
                conn.Execute("DELETE FROM \"event_task\" WHERE \"id\"=@Id",
                    new { Id = model.Id.Value });
            }
        }
    }
}
