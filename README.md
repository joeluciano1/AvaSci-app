**AvaSci Project Overview Document**

**1. Introduction:**

AvaSci is a groundbreaking app that brings the sophistication of motion
tracking and analysis into the palm of your hand. By leveraging the
latest in camera technology, including LiDAR and traditional cameras,
AvaSci offers an unparalleled opportunity to observe and understand the
intricate movements of your body\'s joints. Whether your goal is to
enhance your fitness regime, monitor your rehabilitation journey, or
simply explore your body\'s capabilities, AvaSci provides a
comprehensive and accessible platform.

**1.1 Key Features:**

**1.1.1 Instant Motion Analysis:**

Experience live feedback on your joints\' movements, capturing
everything from flexions and rotations to abductions.

**1.1.2 Progress Tracking:**

Visualize your movement patterns and improvements over time with
intuitive graphs, making it easy to set goals and see results.

**1.1.3 Private and Secure:**

Your data is yours alone. With a robust login system, AvaSci ensures
your information is securely stored, offering peace of mind and privacy.

**1.1.4 User-Friendly:**

Designed for individuals at any stage of their fitness or rehabilitation
journey, AvaSci is intuitive and easy to navigate, regardless of your
tech expertise.

AvaSci is more than an app; it\'s a companion on your journey towards
achieving better movement and understanding your body\'s potential.
Embark on this journey with AvaSci and unlock a new level of body
awareness and improvement.

**2. Development**

The development of the project contains some third party SDKs which are
as following:

1.  LightBuzz

2.  Chart and Graph

3.  Safe area

4.  External Dependancy Manager

5.  Azure Storage

6.  Syncfusion PDF Generator

**2.1 Lightbuzz**

We have partnered with lightbuzz and asked them to create opencv SDK for
us specifically that does reading according to our desires. Following
are the key features of it that should be kept in mind while working:

a.  Screen Capture lidar and normal front camera

b.  Settings view to change different things like switching b/w cameras
    of device

c.  Drop down to select readings you want to do

d.  Recorder view and controls

e.  Player view and controls

f.  CSV generator (its builtin and we cant change it mostly)

**2.1.1 SDK Structure**

In hierarchy of the scene you can see all contents of the package inside
Canvas \>\> AppContainer \>\> Body \>\> Screen1 (Off) \>\>
GraphRecording(off) \>\> LightbuzzPanel.

In project you can see all the Lightbuzz related content inside Assets
\>\> AvaSci. Most of the classes and methods of the SDK are locked but
there are some minor changes that we can do in it according to our
desires.

**2.2 Chart and Graph**

We are using chart and graph unity's SDK from asset store to which we
can feed any values in the x and y axis and it automatically generates
graph for us to view even at runtime and at code behind.

**2.2.1 SDK Structure**

In hierarchy of the scene you can see all contents of the package inside
Canvas \>\> AppContainer \>\> Body \>\> Screen1 (Off) \>\>
GraphRecording(off) \>\> BottomSlidePanel \>\> Scrollview's content \>\>
Graph (off)

In project you can see all the contents of the Graph and chart package
inside Assets \>\> Chart And Graph.

**2.3 Safe Area**

As used by most of the developers Safe Area is used to handle the
devices that have notch on their screens or dynamic islands. All you
need to do is to add the SafeArea.cs on the panel and it will handle the
view accordingly.

**2.4 External Dependancy Manager**

Most of the times we need to add the packages manually because the
builds of the unity gets bugged out we have installed EDM package to
generate xcode workspace rather than xcode project.

**2.5 Azure Storage**

We are using azure blob storage to store the recordings of the users so
they can view them later in their reports records list. In hierarchy of
the scene you can see all contents of the package inside Scripts \>\>
AzureStorageManager

**2.6 Syncfusion PDF Generator**

Syncfusion provides us the ability to create a pdf of our desired
designs and save them in the device after viewing it. In hierarchy of
the scene you can see all contents of the package inside Canvas \>\>
AppContainer \>\> Header \>\> RightSection \>\> Generate PDF

**3. Code Base and Some rules**

There are some keypoints for the developers to keep in mind while
developing following are those:

-   There is no multiple singleton concept in the project only the
    ReferenceManager.cs class is singleton and all the references to
    other classes are passing through it.

-   For the private variables we use camel case and for public variables
    we use pascal case.

-   Methods are always in pascal case no matter private or public.

-   In project hierarchy the manager classes should always be inside
    Scripts gameobject.

-   Using GeneralStaticManager.cs to save global variables like
    downloaded images and API responses. Global variables are
    dictionaries.

-   If you want some action to be performed after showing popups you can
    assign that action inside the okpressed callback.
