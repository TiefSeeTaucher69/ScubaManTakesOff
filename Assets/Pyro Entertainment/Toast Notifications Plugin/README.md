# Pyro Entertainment - Toast Notification - Plugin

## Description

The Toast Notification plugin allow you to create and manage in game notifications in a simple flow. Create several notification types, add them to a queue and trigger them one by one or all at the same time. With 2 lines of code you are able to add and trigger a notification when you need it to appear. Personalize your toasts as you want (by editing the game object prefab). Toast Notification Plugin have built in support for notification display time, notification display interval, image in the notification, color of the image and custom sound. The plugin supports Android, iOS, Web, and standalone builds.


## How to use

Toast Notification plugin uses a singleton prefab to manage in game notification toasts. Drag and drop the **Notification Manager** prefab to your start scene. Note that the object will not be destroyed on scene change; as such, make sure you only add the **Notification Manager** prefab to your game start scene. Select the prefab in your scene and go to the **Notification Manager** script. There you should set the time you want your notifications to be on display, the interval you want for notifications to appear one after each other (if you trigger all of them), and to create the type of notifications you need. Per type, you can add an image, an image color,  sound and the volume you want the sound to play when the notification is triggered. Each notification type should also have a unique name that will be used as an identifier.

You can also personalize your notification. To do so, open the **Notification Toast** prefab and add any elements you need (like a background image or a different image to the time slider), just make sure you **don't remove the Notification Toast script**. Currently, the plugin does not support different toast prefabs.


### Integration

The plugin allows you to add notifications and trigger them when needed. It follows a FIFO (First in First out) logic, making the first notification you add the first that will be shown when you trigger it. Notifications can be added by doing the following:

```
NotificationManager.Instance.AddNotification(<Notification Text>, <Notification Name>);
```

You should replace **<Notification Text>** by the text you want to display and **<Notification Name>** by the type name of the notification you want to trigger (the notification type defined in the notification manager).

You can choose to trigger notifications one by one or all at the same time. The notification will be on display for the amount of time you defined in the Notification Manager. To trigger notifications one by one, call the following:

```
NotificationManager.Instance.TriggerNotification();
```

If you want to trigger all notifications, use the following method:

```
NotificationManager.Instance.TriggerAllNotifications();
```

**Note:** The timing between each notification pops-up can be set in the **Notification Manager**.

### Advance Control

You can have more control over the notifications added by getting a list of the notifications, modifying that list, and setting the list of notifications again. This can be used if you need to remove previously added notifications, change the order of the notifications previously added, or add a range of new notifications.
You can do so by doing:

```
var notifications = NotificationManager.Instance.GetNotifications();
```

To set a new list of notifications, you can all:

```
List<NotificationData> notifications = new List<NotificationData>();
notifications.Add(new NotificationData(<Notification Text>, <Notification Style>))
NotificationManager.Instance.SetNotifications(notifications);
```

**Note:** The order in which you set your notifications will determine the order the notifications will be shown.

You can always create a new style programmatically or, you can get the style from the notification types you have created in the **Notification Manager**. That can be achieved by doing so:
```
NotificationManager.Instance.GetNotificationType(<Notification Name>).Value.style
```

## Support & Contact

If you have any suggestions for improvement or bug reporting, please reach us at:

info@pyroentertainment.com

We hope this helps!