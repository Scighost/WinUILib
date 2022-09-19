// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Windows.Storage;

namespace Scighost.WinUILib.Cache;

/// <summary>
/// Provides methods and tools to cache files in a folder
/// </summary>
public class ImageCache : CacheBase<BitmapImage>
{

    /// <summary>
    /// Private singleton field.
    /// </summary>
    [ThreadStatic]
    private static ImageCache? _instance;

    /// <summary>
    /// Gets public singleton property.
    /// </summary>
    public static ImageCache Instance => _instance ??= new ImageCache();

    /// <summary>
    /// Gets or sets which DispatcherQueue is used to dispatch UI updates.
    /// </summary>
    public DispatcherQueue DispatcherQueue { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageCache"/> class.
    /// </summary>
    /// <param name="dispatcherQueue">The DispatcherQueue that should be used to dispatch UI updates, or null if this is being called from the UI thread.</param>
    public ImageCache(DispatcherQueue? dispatcherQueue = null)
    {
        DispatcherQueue = dispatcherQueue ?? DispatcherQueue.GetForCurrentThread();
    }

    /// <summary>
    /// Cache specific hooks to process items from HTTP response
    /// </summary>
    /// <param name="stream">input stream</param>
    /// <param name="initializerKeyValues">key value pairs used when initializing instance of generic type</param>
    /// <returns>awaitable task</returns>
    protected override async Task<BitmapImage> InitializeTypeAsync(Stream stream, List<KeyValuePair<string, object>>? initializerKeyValues = null)
    {
        if (stream.Length == 0)
        {
            throw new FileNotFoundException();
        }

        return await DispatcherQueue.EnqueueAsync(async () =>
         {
             var image = new BitmapImage();

             if (initializerKeyValues?.Count > 0)
             {
                 foreach (var kvp in initializerKeyValues)
                 {
                     if (string.IsNullOrWhiteSpace(kvp.Key))
                     {
                         continue;
                     }

                     var propInfo = image.GetType().GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance);

                     if (propInfo != null && propInfo.CanWrite)
                     {
                         propInfo.SetValue(image, kvp.Value);
                     }
                 }
             }

             await image.SetSourceAsync(stream.AsRandomAccessStream()).AsTask().ConfigureAwait(false);

             return image;
         });
    }

    /// <summary>
    /// Cache specific hooks to process items from HTTP response
    /// </summary>
    /// <param name="baseFile">storage file</param>
    /// <param name="initializerKeyValues">key value pairs used when initializing instance of generic type</param>
    /// <returns>awaitable task</returns>
    protected override async Task<BitmapImage> InitializeTypeAsync(StorageFile baseFile, List<KeyValuePair<string, object>>? initializerKeyValues = null)
    {
        return await DispatcherQueue.EnqueueAsync(() =>
        {

            var image = new BitmapImage(new(baseFile.Path));

            if (initializerKeyValues?.Count > 0)
            {
                foreach (var kvp in initializerKeyValues)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key))
                    {
                        continue;
                    }

                    var propInfo = image.GetType().GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance);

                    if (propInfo != null && propInfo.CanWrite)
                    {
                        propInfo.SetValue(image, kvp.Value);
                    }
                }
            }

            return Task.FromResult(image);
        });

    }


}


file static class DispatcherQueueExtension
{
    public static Task<T> EnqueueAsync<T>(this DispatcherQueue dispatcher, Func<Task<T>> function, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
    {
        if (dispatcher.HasThreadAccess)
        {
            try
            {
                Task<T> task = function();
                if (task != null)
                {
                    return task;
                }

                return Task.FromException<T>(new InvalidOperationException("The Task returned by function cannot be null."));
            }
            catch (Exception exception)
            {
                return Task.FromException<T>(exception);
            }
        }

        return TryEnqueueAsync(dispatcher, function, priority);
        static Task<T> TryEnqueueAsync(DispatcherQueue dispatcher, Func<Task<T>> function, DispatcherQueuePriority priority)
        {
            Func<Task<T>> function2 = function;
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
            if (!dispatcher.TryEnqueue(priority, async delegate
            {
                try
                {
                    Task<T> task2 = function2();
                    if (task2 != null)
                    {
                        T result = await task2.ConfigureAwait(continueOnCapturedContext: false);
                        taskCompletionSource.SetResult(result);
                    }
                    else
                    {
                        taskCompletionSource.SetException(new InvalidOperationException("The Task returned by function cannot be null."));
                    }
                }
                catch (Exception exception2)
                {
                    taskCompletionSource.SetException(exception2);
                }
            }))
            {
                taskCompletionSource.SetException(new InvalidOperationException("Failed to enqueue the operation"));
            }

            return taskCompletionSource.Task;
        }
    }
}