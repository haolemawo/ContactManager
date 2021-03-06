﻿using ContactManager.App_Start;
using ContactManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;

namespace ContactManager.Controllers
{
    /// <summary>
    /// 联系人
    /// </summary>
    public class ContactController : ApiController
    {
        readonly IContactRepository repository;

        /// <summary>
        /// 联系人构造函数
        /// </summary>
        public ContactController()
        {
            repository = new ContactRepository();
        }

        /// <summary>
        /// 添加联系人
        /// </summary>
        /// <param name="contact">联系人对象</param>
        /// <returns>HttpResponseMessage</returns>
        public HttpResponseMessage Post(Contact contact)
        {
            if(ModelState.IsValid)
            {
                try
                {
                    contact = repository.Add(contact);
                    var response = new HttpResponseMessage(HttpStatusCode.Created);
                    response.Headers.Location = new Uri(Request.RequestUri, "Contact/" + contact.ContactId);
                    return response;
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
                }
            }
            else
            {
                string message = ModelState.Values.FirstOrDefault().Errors[0].ErrorMessage;
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new ArgumentException(message));
            }
        }

        /// <summary>
        /// 根据id获取联系人
        /// </summary>
        /// <param name="id">联系人id</param>
        /// <param name="desc">测试Action过滤器的参数</param>
        /// <returns>HttpResponseMessage</returns>
        [CustomActionFilter]
        [CustomAuthorize(Roles ="User,Test")]
        public HttpResponseMessage Get(int id, string desc = "")
        {
            //如果抛出的异常是HttpResponseException类型的话，会直接输出到客户端
            //throw new Exception("error");
            var response = new HttpResponseMessage();
            Contact contact = repository.Get(id);
            var data = new { contact, desc };
            if(contact == null)
            {
                response.StatusCode = HttpStatusCode.NotFound;
                response.Content = new StringContent("未找到");
                //导致直接 输出到客户端，传入了HttpResponseMessage对象
                throw new HttpResponseException(response);
            }
            else
            {
                response.Content = new ObjectContent(data.GetType(), data, new JsonMediaTypeFormatter());
                return response;
            }
        }

        /// <summary>
        /// 获取多个联系人信息
        /// </summary>
        /// <returns>联系人列表</returns>
        //  api/contact?$filter=substringof(Address,'桃花岛')
        //  api/contact?$orderby=Address&$top=2&$skip=1
        //更多查询条件见 https://msdn.microsoft.com/zh-cn/library/hh169248(v=nav.90).aspx
        [EnableQuery(PageSize = 10)]
        public IQueryable<Contact> Get() => repository.Get().AsQueryable();

        /// <summary>
        /// 删除一个联系人
        /// </summary>
        /// <param name="id">联系人id</param>
        public void Delete(int id) => repository.Delete(id);


        /// <summary>
        /// 修改联系人
        /// </summary>
        /// <param name="contact">联系人对象</param>
        public void Put(Contact contact)
        {
            repository.Update(contact);
        }
    }
}
