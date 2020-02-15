using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BooksCatalogue.Models;
using Microsoft.AspNetCore.Mvc;

namespace BooksCatalogue.Controllers
{
    public class ReviewController : Controller
    {
        private string apiEndpoint = "https://backend-anggakg.azurewebsites.net/api/";
        private readonly HttpClient _client;
        HttpClientHandler clientHandler = new HttpClientHandler();
        public ReviewController() {
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            _client = new HttpClient(clientHandler);
        }

        // GET: Review/AddReview/2
        public async Task<IActionResult> AddReview(int? bookId)
        {
            if (bookId == null)
            {
                return NotFound();
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiEndpoint + "books/" + bookId);

            HttpResponseMessage response = await _client.SendAsync(request);

            switch(response.StatusCode)
            {
                case HttpStatusCode.OK:
                    string responseString = await response.Content.ReadAsStringAsync();
                    var book = JsonSerializer.Deserialize<Book>(responseString);

                    ViewData["BookId"] = bookId;
                    return View("AddReview");
                case HttpStatusCode.NotFound:
                    return NotFound();
                default:
                    return ErrorAction("Error. Status code = " + response.StatusCode + ": " + response.ReasonPhrase);
            }
        }

        // TODO: Tambahkan fungsi ini untuk mengirimkan atau POST data review menuju API
        // POST: Review/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview([Bind("Id,BookId,ReviewerName,Rating,Comment")][FromForm] Review review)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();

            content.Add(new StringContent(review.BookId.ToString()), "bookId");
            content.Add(new StringContent(review.ReviewerName), "reviewerName");
            content.Add(new StringContent(review.Rating.ToString()), "rating");
            content.Add(new StringContent(review.Comment), "comment");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, apiEndpoint);
            request.Content = content;
            HttpResponseMessage response = await _client.SendAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.NoContent:
                case HttpStatusCode.Created:
                    return View("Views/Books/Details.cshtml");
                default:
                    return ErrorAction("Error. Status code = " + response.StatusCode + "; " + response.ReasonPhrase);
            }
        }

        private ActionResult ErrorAction(string message)
        {
            return new RedirectResult("/Home/Error?message=" + message);
        }
    }
}