interface Clipboard {
    writeText(newClipText: string): Promise<void>;
    readText(): Promise<string>;
}
interface NavigatorClipboard {
    readonly clipboard?: Clipboard;
}
interface Navigator extends NavigatorClipboard {
}
declare namespace Uno.Utils {
    class Clipboard {
        private static dispatchContentChanged;
        private static dispatchGetContent;
        static startContentChanged(): void;
        static stopContentChanged(): void;
        static setText(text: string): string;
        static getText(): Promise<string>;
        private static onClipboardChanged;
    }
}
declare namespace Windows.UI.Core {
    /**
     * Support file for the Windows.UI.Core
     * */
    class CoreDispatcher {
        static _coreDispatcherCallback: any;
        static _isFirstCall: boolean;
        static _isReady: Promise<boolean>;
        static _isWaitingReady: boolean;
        static init(isReady: Promise<boolean>): void;
        /**
         * Enqueues a core dispatcher callback on the javascript's event loop
         *
         * */
        static WakeUp(): boolean;
        private static InnerWakeUp;
        private static initMethods;
    }
}
declare namespace Uno.Utils {
    class Guid {
        private static newGuidMethod;
        static NewGuid(): string;
    }
}
declare namespace Uno.UI {
    class HtmlDom {
        /**
         * Initialize various polyfills used by Uno
         */
        static initPolyfills(): void;
        private static isConnectedPolyfill;
    }
}
declare module Uno.UI {
    enum HtmlEventDispatchResult {
        Ok = 0,
        StopPropagation = 1,
        PreventDefault = 2,
        NotDispatched = 128
    }
}
declare namespace Uno.Http {
    interface IHttpClientConfig {
        id: string;
        method: string;
        url: string;
        headers?: string[][];
        payload?: string;
        payloadType?: string;
        cacheMode?: RequestCache;
    }
    class HttpClient {
        static send(config: IHttpClientConfig): Promise<void>;
        private static blobFromBase64;
        private static base64FromBlob;
        private static dispatchResponse;
        private static dispatchError;
        private static dispatchResponseMethod;
        private static dispatchErrorMethod;
        private static initMethods;
    }
}
declare module Uno.UI {
    interface IContentDefinition {
        id: string;
        tagName: string;
        handle: number;
        uiElementRegistrationId: number;
        isSvg: boolean;
        isFocusable: boolean;
    }
}
declare namespace MonoSupport {
    /**
     * This class is used by https://github.com/mono/mono/blob/fa726d3ac7153d87ed187abd422faa4877f85bb5/sdks/wasm/dotnet_support.js#L88 to perform
     * unmarshaled invocation of javascript from .NET code.
     * */
    class jsCallDispatcher {
        private static registrations;
        private static methodMap;
        private static _isUnoRegistered;
        /**
         * Registers a instance for a specified identier
         * @param identifier the scope name
         * @param instance the instance to use for the scope
         */
        static registerScope(identifier: string, instance: any): void;
        static findJSFunction(identifier: string): any;
        /**
         * Internal dispatcher for methods invoked through TSInteropMarshaller
         * @param id The method ID obtained when invoking WebAssemblyRuntime.InvokeJSUnmarshalled with a method name
         * @param pParams The parameters structure ID
         * @param pRet The pointer to the return value structure
         */
        private static dispatch;
        /**
         * Parses the method identifier
         * @param identifier
         */
        private static parseIdentifier;
        /**
         * Adds the a resolved method for a given identifier
         * @param identifier the findJSFunction identifier
         * @param boundMethod the method to call
         */
        private static cacheMethod;
        private static getMethodMapId;
    }
}
declare const config: any;
declare namespace Uno.UI {
    class WindowManager {
        private containerElementId;
        private loadingElementId;
        static current: WindowManager;
        private static _isHosted;
        private static _isLoadEventsEnabled;
        /**
         * Defines if the WindowManager is running in hosted mode, and should skip the
         * initialization of WebAssembly, use this mode in conjunction with the Uno.UI.WpfHost
         * to improve debuggability.
         */
        static get isHosted(): boolean;
        /**
         * Defines if the WindowManager is responsible to raise the loading, loaded and unloaded events,
         * or if they are raised directly by the managed code to reduce interop.
         */
        static get isLoadEventsEnabled(): boolean;
        private static readonly unoRootClassName;
        private static readonly unoUnarrangedClassName;
        private static readonly unoCollapsedClassName;
        private static _cctor;
        /**
            * Initialize the WindowManager
            * @param containerElementId The ID of the container element for the Xaml UI
            * @param loadingElementId The ID of the loading element to remove once ready
            */
        static init(isHosted: boolean, isLoadEventsEnabled: boolean, containerElementId?: string, loadingElementId?: string): string;
        /**
         * Builds a promise that will signal the ability for the dispatcher
         * to initiate work.
         * */
        private static buildReadyPromise;
        /**
         * Build the splashscreen image eagerly
         * */
        private static buildSplashScreen;
        /**
            * Initialize the WindowManager
            * @param containerElementId The ID of the container element for the Xaml UI
            * @param loadingElementId The ID of the loading element to remove once ready
            */
        static initNative(pParams: number): boolean;
        private containerElement;
        private rootContent;
        private cursorStyleElement;
        private allActiveElementsById;
        private uiElementRegistrations;
        private static resizeMethod;
        private static dispatchEventMethod;
        private static focusInMethod;
        private static dispatchSuspendingMethod;
        private static getDependencyPropertyValueMethod;
        private static setDependencyPropertyValueMethod;
        private constructor();
        /**
            * Creates the UWP-compatible splash screen
            *
            */
        static setupSplashScreen(splashImage: HTMLImageElement): void;
        /**
            * Reads the window's search parameters
            *
            */
        static findLaunchArguments(): string;
        /**
            * Create a html DOM element representing a Xaml element.
            *
            * You need to call addView to connect it to the DOM.
            */
        createContent(contentDefinition: IContentDefinition): string;
        /**
            * Create a html DOM element representing a Xaml element.
            *
            * You need to call addView to connect it to the DOM.
            */
        createContentNative(pParams: number): boolean;
        private createContentInternal;
        registerUIElement(typeName: string, isFrameworkElement: boolean, classNames: string[]): number;
        registerUIElementNative(pParams: number, pReturn: number): boolean;
        getView(elementHandle: number): HTMLElement | SVGElement;
        /**
            * Set a name for an element.
            *
            * This is mostly for diagnostic purposes.
            */
        setName(elementId: number, name: string): string;
        /**
            * Set a name for an element.
            *
            * This is mostly for diagnostic purposes.
            */
        setNameNative(pParam: number): boolean;
        private setNameInternal;
        /**
            * Set a name for an element.
            *
            * This is mostly for diagnostic purposes.
            */
        setXUid(elementId: number, name: string): string;
        /**
            * Set a name for an element.
            *
            * This is mostly for diagnostic purposes.
            */
        setXUidNative(pParam: number): boolean;
        private setXUidInternal;
        /**
            * Sets the visibility of the specified element
            */
        setVisibility(elementId: number, visible: boolean): string;
        setVisibilityNative(pParam: number): boolean;
        private setVisibilityInternal;
        /**
            * Set an attribute for an element.
            */
        setAttributes(elementId: number, attributes: {
            [name: string]: string;
        }): string;
        /**
            * Set an attribute for an element.
            */
        setAttributesNative(pParams: number): boolean;
        /**
            * Set an attribute for an element.
            */
        setAttributeNative(pParams: number): boolean;
        /**
            * Removes an attribute for an element.
            */
        removeAttribute(elementId: number, name: string): string;
        /**
            * Removes an attribute for an element.
            */
        removeAttributeNative(pParams: number): boolean;
        /**
            * Get an attribute for an element.
            */
        getAttribute(elementId: number, name: string): any;
        /**
            * Set a property for an element.
            */
        setProperty(elementId: number, properties: {
            [name: string]: string;
        }): string;
        /**
            * Set a property for an element.
            */
        setPropertyNative(pParams: number): boolean;
        /**
            * Get a property for an element.
            */
        getProperty(elementId: number, name: string): any;
        /**
            * Set the CSS style of a html element.
            *
            * To remove a value, set it to empty string.
            * @param styles A dictionary of styles to apply on html element.
            */
        setStyle(elementId: number, styles: {
            [name: string]: string;
        }): string;
        /**
        * Set the CSS style of a html element.
        *
        * To remove a value, set it to empty string.
        * @param styles A dictionary of styles to apply on html element.
        */
        setStyleNative(pParams: number): boolean;
        /**
        * Set a single CSS style of a html element
        *
        */
        setStyleDoubleNative(pParams: number): boolean;
        setArrangeProperties(elementId: number): string;
        /**
            * Remove the CSS style of a html element.
            */
        resetStyle(elementId: number, names: string[]): string;
        /**
            * Remove the CSS style of a html element.
            */
        resetStyleNative(pParams: number): boolean;
        private resetStyleInternal;
        isCssPropertySupported(propertyName: string, value: string): boolean;
        isCssConditionSupported(supportCondition: string): boolean;
        /**
         * Set + Unset CSS classes on an element
         */
        setUnsetClasses(elementId: number, cssClassesToSet: string[], cssClassesToUnset: string[]): void;
        setUnsetClassesNative(pParams: number): boolean;
        /**
         * Set CSS classes on an element from a specified list
         */
        setClasses(elementId: number, cssClassesList: string[], classIndex: number): string;
        setClassesNative(pParams: number): boolean;
        /**
        * Arrange and clips a native elements
        *
        */
        arrangeElementNative(pParams: number): boolean;
        private setAsArranged;
        private setAsUnarranged;
        /**
        * Sets the color property of the specified element
        */
        setElementColor(elementId: number, color: number): string;
        setElementColorNative(pParam: number): boolean;
        private setElementColorInternal;
        /**
        * Sets the fill property of the specified element
        */
        setElementFill(elementId: number, color: number): string;
        setElementFillNative(pParam: number): boolean;
        private setElementFillInternal;
        /**
        * Sets the background color property of the specified element
        */
        setElementBackgroundColor(pParam: number): boolean;
        /**
        * Sets the background image property of the specified element
        */
        setElementBackgroundGradient(pParam: number): boolean;
        /**
        * Clears the background property of the specified element
        */
        resetElementBackground(pParam: number): boolean;
        /**
        * Sets the transform matrix of an element
        *
        */
        setElementTransformNative(pParams: number): boolean;
        private setPointerEvents;
        setPointerEventsNative(pParams: number): boolean;
        /**
            * Load the specified URL into a new tab or window
            * @param url URL to load
            * @returns "True" or "False", depending on whether a new window could be opened or not
            */
        open(url: string): string;
        /**
            * Issue a browser alert to user
            * @param message message to display
            */
        alert(message: string): string;
        /**
            * Sets the browser window title
            * @param message the new title
            */
        setWindowTitle(title: string): string;
        /**
            * Gets the currently set browser window title
            */
        getWindowTitle(): string;
        /**
            * Add an event handler to a html element.
            *
            * @param eventName The name of the event
            * @param onCapturePhase true means "on trickle down" (going down to target), false means "on bubble up" (bubbling back to ancestors). Default is false.
            */
        registerEventOnView(elementId: number, eventName: string, onCapturePhase: boolean, eventExtractorId: number): string;
        /**
            * Add an event handler to a html element.
            *
            * @param eventName The name of the event
            * @param onCapturePhase true means "on trickle down", false means "on bubble up". Default is false.
            */
        registerEventOnViewNative(pParams: number): boolean;
        /**
            * Add an event handler to a html element.
            *
            * @param eventName The name of the event
            * @param onCapturePhase true means "on trickle down", false means "on bubble up". Default is false.
            */
        private registerEventOnViewInternal;
        /**
         * keyboard event extractor to be used with registerEventOnView
         * @param evt
         */
        private keyboardEventExtractor;
        /**
         * tapped (mouse clicked / double clicked) event extractor to be used with registerEventOnView
         * @param evt
         */
        private tappedEventExtractor;
        /**
         * focus event extractor to be used with registerEventOnView
         * @param evt
         */
        private focusEventExtractor;
        private customEventDetailExtractor;
        private customEventDetailStringExtractor;
        /**
         * Gets the event extractor function. See UIElement.HtmlEventExtractor
         * @param eventExtractorName an event extractor name.
         */
        private getEventExtractor;
        /**
            * Set or replace the root content element.
            */
        setRootContent(elementId?: number): string;
        /**
            * Set a view as a child of another one.
            *
            * "Loading" & "Loaded" events will be raised if necessary.
            *
            * @param index Position in children list. Appended at end if not specified.
            */
        addView(parentId: number, childId: number, index?: number): string;
        /**
            * Set a view as a child of another one.
            *
            * "Loading" & "Loaded" events will be raised if necessary.
            *
            * @param pParams Pointer to a WindowManagerAddViewParams native structure.
            */
        addViewNative(pParams: number): boolean;
        addViewInternal(parentId: number, childId: number, index?: number): void;
        /**
            * Remove a child from a parent element.
            *
            * "Unloading" & "Unloaded" events will be raised if necessary.
            */
        removeView(parentId: number, childId: number): string;
        /**
            * Remove a child from a parent element.
            *
            * "Unloading" & "Unloaded" events will be raised if necessary.
            */
        removeViewNative(pParams: number): boolean;
        private removeViewInternal;
        /**
            * Destroy a html element.
            *
            * The element won't be available anymore. Usually indicate the managed
            * version has been scavenged by the GC.
            */
        destroyView(elementId: number): string;
        /**
            * Destroy a html element.
            *
            * The element won't be available anymore. Usually indicate the managed
            * version has been scavenged by the GC.
            */
        destroyViewNative(pParams: number): boolean;
        private destroyViewInternal;
        getBoundingClientRect(elementId: number): string;
        getBBox(elementId: number): string;
        getBBoxNative(pParams: number, pReturn: number): boolean;
        private getBBoxInternal;
        setSvgElementRect(pParams: number): boolean;
        /**
            * Use the Html engine to measure the element using specified constraints.
            *
            * @param maxWidth string containing width in pixels. Empty string means infinite.
            * @param maxHeight string containing height in pixels. Empty string means infinite.
            * @param measureContent if we're interested by the content of the control (<img>'s image, <input>'s text...)
            */
        measureView(viewId: string, maxWidth: string, maxHeight: string, measureContent?: boolean): string;
        /**
            * Use the Html engine to measure the element using specified constraints.
            *
            * @param maxWidth string containing width in pixels. Empty string means infinite.
            * @param maxHeight string containing height in pixels. Empty string means infinite.
            */
        measureViewNative(pParams: number, pReturn: number): boolean;
        private static MAX_WIDTH;
        private static MAX_HEIGHT;
        private measureElement;
        private measureViewInternal;
        private createUnconstrainedStyle;
        scrollTo(pParams: number): boolean;
        rawPixelsToBase64EncodeImage(dataPtr: number, width: number, height: number): string;
        /**
         * Sets the provided image with a mono-chrome version of the provided url.
         * @param viewId the image to manipulate
         * @param url the source image
         * @param color the color to apply to the monochrome pixels
         */
        setImageAsMonochrome(viewId: number, url: string, color: string): string;
        setPointerCapture(viewId: number, pointerId: number): string;
        releasePointerCapture(viewId: number, pointerId: number): string;
        focusView(elementId: number): string;
        /**
            * Set the Html content for an element.
            *
            * Those html elements won't be available as XamlElement in managed code.
            * WARNING: you should avoid mixing this and `addView` for the same element.
            */
        setHtmlContent(viewId: number, html: string): string;
        /**
            * Set the Html content for an element.
            *
            * Those html elements won't be available as XamlElement in managed code.
            * WARNING: you should avoid mixing this and `addView` for the same element.
            */
        setHtmlContentNative(pParams: number): boolean;
        private setHtmlContentInternal;
        /**
         * Gets the Client and Offset size of the specified element
         *
         * This method is used to determine the size of the scroll bars, to
         * mask the events coming from that zone.
         */
        getClientViewSize(elementId: number): string;
        /**
         * Gets the Client and Offset size of the specified element
         *
         * This method is used to determine the size of the scroll bars, to
         * mask the events coming from that zone.
         */
        getClientViewSizeNative(pParams: number, pReturn: number): boolean;
        /**
         * Gets a dependency property value.
         *
         * Note that the casing of this method is intentionally Pascal for platform alignment.
         */
        GetDependencyPropertyValue(elementId: number, propertyName: string): string;
        /**
         * Sets a dependency property value.
         *
         * Note that the casing of this method is intentionally Pascal for platform alignment.
         */
        SetDependencyPropertyValue(elementId: number, propertyNameAndValue: string): string;
        /**
            * Remove the loading indicator.
            *
            * In a future version it will also handle the splashscreen.
            */
        activate(): string;
        private init;
        private static initMethods;
        private initDom;
        private removeLoading;
        private resize;
        private onfocusin;
        private onWindowBlur;
        private dispatchEvent;
        private getIsConnectedToRootElement;
        private handleToString;
        private numberToCssColor;
        setCursor(cssCursor: string): string;
        getNaturalImageSize(imageUrl: string): Promise<string>;
        selectInputRange(elementId: number, start: number, length: number): void;
    }
}
interface PointerEvent {
    isOver(this: PointerEvent, element: HTMLElement | SVGElement): boolean;
    isOverDeep(this: PointerEvent, element: HTMLElement | SVGElement): boolean;
    /**
     * Indicates if the pointer is over the given 'element' and there no other element above it (i.e. given 'element' is top most).
     */
    isDirectlyOver(this: PointerEvent, element: HTMLElement | SVGElement): boolean;
}
declare namespace Uno.UI.Interop {
    class AsyncInteropHelper {
        private static dispatchResultMethod;
        private static dispatchErrorMethod;
        private static init;
        static Invoke(handle: number, promiseFunction: () => Promise<string>): void;
    }
}
declare module Uno.UI {
    interface IAppManifest {
        splashScreenImage: URL;
        splashScreenColor: string;
        displayName: string;
    }
}
declare module Uno.UI.Interop {
    interface IMonoAssemblyHandle {
    }
}
declare module Uno.UI.Interop {
    interface IMonoClassHandle {
    }
}
declare module Uno.UI.Interop {
    interface IMonoMethodHandle {
    }
}
declare module Uno.UI.Interop {
    interface IMonoRuntime {
        assembly_load(assemblyName: string): Interop.IMonoAssemblyHandle;
        find_class(moduleHandle: Interop.IMonoAssemblyHandle, namespace: string, typeName: string): Interop.IMonoClassHandle;
        find_method(classHandle: Interop.IMonoClassHandle, methodName: string, _: number): Interop.IMonoMethodHandle;
        call_method(methodHandle: Interop.IMonoMethodHandle, object: any, params?: any[]): any;
        mono_string(str: string): Interop.IMonoStringHandle;
        conv_string(strHandle: Interop.IMonoStringHandle): string;
    }
}
declare module Uno.UI.Interop {
    interface IMonoStringHandle {
    }
}
declare module Uno.UI.Interop {
    interface IUnoDispatch {
        resize(size: string): void;
        dispatch(htmlIdStr: string, eventNameStr: string, eventPayloadStr: string): string;
    }
}
declare module Uno.UI.Interop {
    interface IWebAssemblyApp {
        main_module: Interop.IMonoAssemblyHandle;
        main_class: Interop.IMonoClassHandle;
    }
}
declare namespace Uno.Foundation.Interop {
    class ManagedObject {
        private static assembly;
        private static dispatchMethod;
        private static init;
        static dispatch(handle: string, method: string, parameters: string): void;
    }
}
declare namespace Uno.UI.Interop {
    class Runtime {
        static readonly engine: any;
        private static init;
    }
}
declare namespace Uno.UI.Interop {
    class Xaml {
    }
}
declare const MonoRuntime: Uno.UI.Interop.IMonoRuntime;
declare const WebAssemblyApp: Uno.UI.Interop.IWebAssemblyApp;
declare const UnoAppManifest: Uno.UI.IAppManifest;
declare const UnoDispatch: Uno.UI.Interop.IUnoDispatch;
declare enum ContactProperty {
    Address = "address",
    Email = "email",
    Icon = "icon",
    Name = "name",
    Tel = "tel"
}
declare class ContactInfo {
    address: PaymentAddress[];
    email: string[];
    name: string;
    tel: string;
}
declare class ContactsManager {
    select(props: ContactProperty[], options: any): Promise<ContactInfo[]>;
}
interface Navigator {
    contacts: ContactsManager;
}
declare namespace Windows.ApplicationModel.Contacts {
    class ContactPicker {
        static isSupported(): boolean;
        static pickContacts(pickMultiple: boolean): Promise<string>;
    }
}
interface NavigatorDataTransferManager {
    share(data: any): Promise<void>;
}
interface Navigator extends NavigatorDataTransferManager {
}
declare namespace Windows.ApplicationModel.DataTransfer {
    class DataTransferManager {
        static isSupported(): boolean;
        static showShareUI(title: string, text: string, url: string): Promise<string>;
    }
}
declare namespace Windows.ApplicationModel.DataTransfer.DragDrop.Core {
    class DragDropExtension {
        private static _dispatchDropEventMethod;
        private static _dispatchDragDropArgs;
        private static _current;
        private static _nextDropId;
        private _dropHandler;
        private _pendingDropId;
        private _pendingDropData;
        static enable(pArgs: number): void;
        static disable(pArgs: number): void;
        constructor();
        dispose(): void;
        private dispatchDropEvent;
        static retrieveText(itemId: number): Promise<string>;
        static retrieveFiles(itemIds: number | number[]): Promise<string>;
        private static getAsFile;
    }
}
declare namespace Uno.Devices.Enumeration.Internal.Providers.Midi {
    class MidiDeviceClassProvider {
        static findDevices(findInputDevices: boolean): string;
    }
}
declare namespace Uno.Devices.Enumeration.Internal.Providers.Midi {
    class MidiDeviceConnectionWatcher {
        private static dispatchStateChanged;
        static startStateChanged(): void;
        static stopStateChanged(): void;
        static onStateChanged(event: WebMidi.MIDIConnectionEvent): void;
    }
}
declare namespace Windows.Devices.Geolocation {
    class Geolocator {
        private static dispatchAccessRequest;
        private static dispatchGeoposition;
        private static dispatchError;
        private static positionWatches;
        static initialize(): void;
        static requestAccess(): void;
        static getGeoposition(desiredAccuracyInMeters: number, maximumAge: number, timeout: number, requestId: string): void;
        static startPositionWatch(desiredAccuracyInMeters: number, requestId: string): boolean;
        static stopPositionWatch(desiredAccuracyInMeters: number, requestId: string): void;
        private static handleGeoposition;
        private static handleError;
        private static getAccurateCurrentPosition;
    }
}
declare module Windows.Devices.Input {
    enum PointerDeviceType {
        Touch = 0,
        Pen = 1,
        Mouse = 2
    }
}
declare namespace Windows.Devices.Midi {
    class MidiInPort {
        private static dispatchMessage;
        private static instanceMap;
        private managedId;
        private inputPort;
        private constructor();
        static createPort(managedId: string, encodedDeviceId: string): void;
        static removePort(managedId: string): void;
        static startMessageListener(managedId: string): void;
        static stopMessageListener(managedId: string): void;
        private messageReceived;
    }
}
declare namespace Windows.Devices.Midi {
    class MidiOutPort {
        static sendBuffer(encodedDeviceId: string, timestamp: number, ...args: number[]): void;
    }
}
declare namespace Uno.Devices.Midi.Internal {
    class WasmMidiAccess {
        private static midiAccess;
        static request(systemExclusive: boolean): Promise<string>;
        static getMidi(): WebMidi.MIDIAccess;
    }
}
interface Window {
    DeviceMotionEvent(): void;
}
declare namespace Windows.Devices.Sensors {
    class Accelerometer {
        private static dispatchReading;
        static initialize(): boolean;
        static startReading(): void;
        static stopReading(): void;
        private static readingChangedHandler;
    }
}
declare class Gyroscope {
    constructor(config: any);
    addEventListener(type: "reading" | "activate", listener: (this: this, ev: Event) => any, useCapture?: boolean): void;
}
interface Window {
    Gyroscope: Gyroscope;
}
declare namespace Windows.Devices.Sensors {
    class Gyrometer {
        private static dispatchReading;
        private static gyroscope;
        static initialize(): boolean;
        static startReading(): void;
        static stopReading(): void;
        private static readingChangedHandler;
    }
}
declare class AmbientLightSensor {
    constructor(config: any);
    addEventListener(type: "reading", listener: (this: this, ev: Event) => any): void;
    removeEventListener(type: "reading", listener: (this: this, ev: Event) => any): void;
    start(): void;
    stop(): void;
    illuminance: number;
}
interface Window {
    AmbientLightSensor: AmbientLightSensor;
}
declare namespace Windows.Devices.Sensors {
    class LightSensor {
        private static dispatchReading;
        private static ambientLightSensor;
        static initialize(): boolean;
        static startReading(): void;
        static stopReading(): void;
        private static readingChangedHandler;
    }
}
declare class Magnetometer {
    constructor(config: any);
    addEventListener(type: "reading" | "activate", listener: (this: this, ev: Event) => any, useCapture?: boolean): void;
}
interface Window {
    Magnetometer: Magnetometer;
}
declare namespace Windows.Devices.Sensors {
    class Magnetometer {
        private static dispatchReading;
        private static magnetometer;
        static initialize(): boolean;
        static startReading(): void;
        static stopReading(): void;
        private static readingChangedHandler;
    }
}
declare namespace Windows.Graphics.Display {
    class DisplayInformation {
        private static readonly DpiCheckInterval;
        private static lastDpi;
        private static dpiWatcher;
        private static dispatchOrientationChanged;
        private static dispatchDpiChanged;
        static startOrientationChanged(): void;
        static stopOrientationChanged(): void;
        static startDpiChanged(): void;
        static stopDpiChanged(): void;
        private static updateDpi;
        private static onOrientationChange;
    }
}
interface Window {
    SpeechRecognition: any;
    webkitSpeechRecognition: any;
}
declare namespace Windows.Media {
    class SpeechRecognizer {
        private static dispatchResult;
        private static dispatchHypothesis;
        private static dispatchStatus;
        private static dispatchError;
        private static instanceMap;
        private managedId;
        private recognition;
        private constructor();
        static initialize(managedId: string, culture: string): void;
        static recognize(managedId: string): boolean;
        static removeInstance(managedId: string): void;
        private onResult;
        private onSpeechStart;
        private onError;
    }
}
declare namespace Windows.Networking.Connectivity {
    class ConnectionProfile {
        static hasInternetAccess(): boolean;
    }
}
declare namespace Windows.Networking.Connectivity {
    class NetworkInformation {
        private static dispatchStatusChanged;
        static startStatusChanged(): void;
        static stopStatusChanged(): void;
        static networkStatusChanged(): void;
    }
}
interface Navigator {
    webkitVibrate(pattern: number | number[]): boolean;
    mozVibrate(pattern: number | number[]): boolean;
    msVibrate(pattern: number | number[]): boolean;
}
declare namespace Windows.Phone.Devices.Notification {
    class VibrationDevice {
        static initialize(): boolean;
        static vibrate(duration: number): boolean;
    }
}
declare namespace Windows.Security.Authentication.Web {
    class WebAuthenticationBroker {
        static getReturnUrl(): string;
        static authenticateUsingIframe(iframeId: string, urlNavigate: string, urlRedirect: string, timeout: number): Promise<string>;
        static authenticateUsingWindow(urlNavigate: string, urlRedirect: string, title: string, popUpWidth: number, popUpHeight: number, timeout: number): Promise<string>;
        private static startMonitoringRedirect;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer {
        private static buildStorageKey;
        private static buildStoragePrefix;
        /**
         * Try to get a value from localStorage
         * */
        private static tryGetValue;
        /**
         * Set a value to localStorage
         * */
        private static setValue;
        /**
         * Determines if a key is contained in localStorage
         * */
        private static containsKey;
        /**
         * Gets a key by index in localStorage
         * */
        private static getKeyByIndex;
        /**
         * Determines the number of items contained in localStorage
         * */
        private static getCount;
        /**
         * Clears items contained in localStorage
         * */
        private static clear;
        /**
         * Removes an item contained in localStorage
         * */
        private static remove;
        /**
         * Gets a key by index in localStorage
         * */
        private static getValueByIndex;
    }
}
declare namespace Windows.Storage {
    class AssetManager {
        static DownloadAssetsManifest(path: string): Promise<string>;
        static DownloadAsset(path: string): Promise<string>;
    }
}
declare namespace Uno.Storage {
    class NativeStorageFile {
        static getBasicPropertiesAsync(guid: string): Promise<string>;
    }
}
declare namespace Uno.Storage {
    class NativeStorageFolder {
        /**
         * Creates a new folder inside another folder.
         * @param parentGuid The GUID of the folder to create in.
         * @param folderName The name of the new folder.
         */
        static createFolderAsync(parentGuid: string, folderName: string): Promise<string>;
        /**
         * Creates a new file inside another folder.
         * @param parentGuid The GUID of the folder to create in.
         * @param folderName The name of the new file.
         */
        static createFileAsync(parentGuid: string, fileName: string): Promise<string>;
        /**
         * Tries to get a folder in the given parent folder by name.
         * @param parentGuid The GUID of the parent folder to get.
         * @param folderName The name of the folder to look for.
         * @returns A GUID of the folder if found, otherwise null.
         */
        static tryGetFolderAsync(parentGuid: string, folderName: string): Promise<string>;
        /**
        * Tries to get a file in the given parent folder by name.
        * @param parentGuid The GUID of the parent folder to get.
        * @param folderName The name of the folder to look for.
        * @returns A GUID of the folder if found, otherwise null.
        */
        static tryGetFileAsync(parentGuid: string, fileName: string): Promise<string>;
        static deleteItemAsync(parentGuid: string, itemName: string): Promise<string>;
        static getItemsAsync(folderGuid: string): Promise<string>;
        static getFoldersAsync(folderGuid: string): Promise<string>;
        static getFilesAsync(folderGuid: string): Promise<string>;
        static getPrivateRootAsync(): Promise<string>;
        private static getEntriesAsync;
    }
}
declare namespace Uno.Storage {
    class NativeStorageItem {
        private static generateGuidBinding;
        private static _guidToItemMap;
        private static _itemToGuidMap;
        static addItem(guid: string, item: FileSystemHandle | File): void;
        static removeItem(guid: string): void;
        static getItem(guid: string): FileSystemHandle | File;
        static getFile(guid: string): Promise<File>;
        static getGuid(item: FileSystemHandle | File): string;
        static getInfos(...items: Array<FileSystemHandle | File>): NativeStorageItemInfo[];
        private static storeItems;
        private static generateGuids;
    }
}
declare namespace Uno.Storage {
    class NativeStorageItemInfo {
        id: string;
        name: string;
        path: string;
        isFile: boolean;
    }
}
declare namespace Windows.Storage {
    class StorageFolder {
        private static _isInitialized;
        private static _isSynchronizing;
        private static dispatchStorageInitialized;
        /**
         * Determine if IndexDB is available, some browsers and modes disable it.
         * */
        static isIndexDBAvailable(): boolean;
        /**
         * Setup the storage persistence of a given set of paths.
         * */
        private static makePersistent;
        /**
         * Setup the storage persistence of a given path.
         * */
        static setupStorage(path: string): void;
        private static onStorageInitialized;
        /**
         * Synchronize the IDBFS memory cache back to IndexedDB
         * populate: requests the filesystem to be popuplated from the IndexedDB
         * onSynchronized: function invoked when the synchronization finished
         * */
        private static synchronizeFileSystem;
    }
}
declare namespace Windows.Storage.Pickers {
    class FileOpenPicker {
        static isNativeSupported(): boolean;
        static nativePickFilesAsync(multiple: boolean, showAllEntry: boolean, fileTypesJson: string, id: string, startIn: StartInDirectory): Promise<string>;
        static uploadPickFilesAsync(multiple: boolean, targetPath: string, accept: string): Promise<string>;
    }
}
declare namespace Windows.Storage.Pickers {
    class FileSavePicker {
        static isNativeSupported(): boolean;
        static nativePickSaveFileAsync(showAllEntry: boolean, fileTypesJson: string, suggestedFileName: string, id: string, startIn: StartInDirectory): Promise<string>;
        static SaveAs(fileName: string, dataPtr: any, size: number): void;
    }
}
declare namespace Windows.Storage.Pickers {
    class FolderPicker {
        static isNativeSupported(): boolean;
        static pickSingleFolderAsync(id: string, startIn: StartInDirectory): Promise<string>;
    }
}
declare namespace Uno.Storage.Pickers {
    class NativeFilePickerAcceptType {
        description: string;
        accept: NativeFilePickerAcceptTypeItem[];
    }
}
declare namespace Uno.Storage.Pickers {
    class NativeFilePickerAcceptTypeItem {
        mimeType: string;
        extensions: string[];
    }
}
declare namespace Uno.Storage.Streams {
    class NativeFileReadStream {
        private static _streamMap;
        private _file;
        private constructor();
        static openAsync(streamId: string, fileId: string): Promise<string>;
        static readAsync(streamId: string, targetArrayPointer: number, offset: number, count: number, position: number): Promise<string>;
        static close(streamId: string): void;
    }
}
declare namespace Uno.Storage.Streams {
    class NativeFileWriteStream {
        private static _streamMap;
        private _stream;
        private _buffer;
        private constructor();
        static openAsync(streamId: string, fileId: string): Promise<string>;
        private static verifyPermissionAsync;
        static writeAsync(streamId: string, dataArrayPointer: number, offset: number, count: number, position: number): Promise<string>;
        static closeAsync(streamId: string): Promise<string>;
        static truncateAsync(streamId: string, length: number): Promise<string>;
    }
}
declare namespace Windows.System {
    class MemoryManager {
        static getAppMemoryUsage(): any;
    }
}
interface Navigator {
    wakeLock: WakeLock;
}
declare enum WakeLockType {
    screen = "screen"
}
interface WakeLock {
    request(type: WakeLockType): Promise<WakeLockSentinel>;
}
interface WakeLockSentinel {
    release(): Promise<void>;
}
declare namespace Windows.System.Display {
    class DisplayRequest {
        private static activeScreenLockPromise;
        static activateScreenLock(): void;
        static deactivateScreenLock(): void;
    }
}
declare namespace Windows.System.Profile {
    class AnalyticsInfo {
        static getDeviceType(): string;
    }
}
interface Window {
    opr: any;
    opera: any;
    mozVibrate(pattern: number | number[]): boolean;
    msVibrate(pattern: number | number[]): boolean;
    InstallTrigger: any;
    HTMLElement: any;
    StyleMedia: any;
    chrome: any;
    CSS: any;
    safari: any;
}
interface Document {
    documentMode: any;
}
declare namespace Windows.System.Profile {
    class AnalyticsVersionInfo {
        static getUserAgent(): string;
        static getBrowserName(): string;
    }
}
declare namespace Windows.UI.Core {
    class SystemNavigationManager {
        private static _current;
        static get current(): SystemNavigationManager;
        private _isEnabled;
        constructor();
        enable(): void;
        disable(): void;
        private clearStack;
    }
}
interface Navigator {
    setAppBadge(value: number): void;
    clearAppBadge(): void;
}
declare namespace Windows.UI.Notifications {
    class BadgeUpdater {
        static setNumber(value: number): void;
        static clear(): void;
    }
}
declare namespace Windows.UI.ViewManagement {
    class ApplicationView {
        static setFullScreenMode(turnOn: boolean): boolean;
    }
}
declare namespace Windows.UI.ViewManagement {
    class ApplicationViewTitleBar {
        static setBackgroundColor(colorString: string): void;
    }
}
declare namespace Windows.UI.Xaml {
    class Application {
        private static dispatchThemeChange;
        private static dispatchVisibilityChange;
        static getDefaultSystemTheme(): string;
        static observeSystemTheme(): void;
        static observeVisibility(): void;
    }
}
declare namespace Windows.UI.Xaml {
    enum ApplicationTheme {
        Light = "Light",
        Dark = "Dark"
    }
}
declare namespace Windows.UI.Xaml {
    enum NativePointerEvent {
        pointerover = 1,
        pointerout = 2,
        pointerdown = 4,
        pointerup = 8,
        pointercancel = 16,
        pointermove = 32,
        wheel = 64
    }
    class UIElement_Pointers {
        private static _dispatchPointerEventMethod;
        private static _dispatchPointerEventArgs;
        private static _dispatchPointerEventResult;
        static setPointerEventArgs(pArgs: number): void;
        static setPointerEventResult(pArgs: number): void;
        static subscribePointerEvents(pParams: number): void;
        static unSubscribePointerEvents(pParams: number): void;
        private static onPointerEventReceived;
        private static onPointerOutReceived;
        private static dispatchPointerEvent;
        private static _wheelLineSize;
        private static get wheelLineSize();
        private static toNativePointerEventArgs;
        private static toNativeEvent;
        private static toPointerDeviceType;
    }
}
declare namespace Windows.UI.Xaml {
    class UIElement extends Windows.UI.Xaml.UIElement_Pointers {
    }
}
declare namespace Windows.UI.Xaml.Media.Animation {
    class RenderingLoopAnimator {
        private managedHandle;
        private static activeInstances;
        static createInstance(managedHandle: string, jsHandle: number): void;
        static getInstance(jsHandle: number): RenderingLoopAnimator;
        static destroyInstance(jsHandle: number): void;
        private constructor();
        SetStartFrameDelay(delay: number): void;
        SetAnimationFramesInterval(): void;
        EnableFrameReporting(): void;
        DisableFrameReporting(): void;
        private onFrame;
        private unscheduleFrame;
        private scheduleDelayedFrame;
        private scheduleAnimationFrame;
        private _delayRequestId?;
        private _frameRequestId?;
        private _isEnabled;
    }
}
declare class WindowManagerAddViewParams {
    HtmlId: number;
    ChildView: number;
    Index: number;
    static unmarshal(pData: number): WindowManagerAddViewParams;
}
declare class WindowManagerArrangeElementParams {
    Top: number;
    Left: number;
    Width: number;
    Height: number;
    ClipTop: number;
    ClipLeft: number;
    ClipBottom: number;
    ClipRight: number;
    HtmlId: number;
    Clip: boolean;
    static unmarshal(pData: number): WindowManagerArrangeElementParams;
}
declare class WindowManagerCreateContentParams {
    HtmlId: number;
    TagName: string;
    Handle: number;
    UIElementRegistrationId: number;
    IsSvg: boolean;
    IsFocusable: boolean;
    static unmarshal(pData: number): WindowManagerCreateContentParams;
}
declare class WindowManagerDestroyViewParams {
    HtmlId: number;
    static unmarshal(pData: number): WindowManagerDestroyViewParams;
}
declare class WindowManagerGetBBoxParams {
    HtmlId: number;
    static unmarshal(pData: number): WindowManagerGetBBoxParams;
}
declare class WindowManagerGetBBoxReturn {
    X: number;
    Y: number;
    Width: number;
    Height: number;
    marshal(pData: number): void;
}
declare class WindowManagerGetClientViewSizeParams {
    HtmlId: number;
    static unmarshal(pData: number): WindowManagerGetClientViewSizeParams;
}
declare class WindowManagerGetClientViewSizeReturn {
    OffsetWidth: number;
    OffsetHeight: number;
    ClientWidth: number;
    ClientHeight: number;
    marshal(pData: number): void;
}
declare class WindowManagerInitParams {
    IsHostedMode: boolean;
    IsLoadEventsEnabled: boolean;
    static unmarshal(pData: number): WindowManagerInitParams;
}
declare class WindowManagerMeasureViewParams {
    HtmlId: number;
    AvailableWidth: number;
    AvailableHeight: number;
    MeasureContent: boolean;
    static unmarshal(pData: number): WindowManagerMeasureViewParams;
}
declare class WindowManagerMeasureViewReturn {
    DesiredWidth: number;
    DesiredHeight: number;
    marshal(pData: number): void;
}
declare class WindowManagerRegisterEventOnViewParams {
    HtmlId: number;
    EventName: string;
    OnCapturePhase: boolean;
    EventExtractorId: number;
    static unmarshal(pData: number): WindowManagerRegisterEventOnViewParams;
}
declare class WindowManagerRegisterPointerEventsOnViewParams {
    HtmlId: number;
    static unmarshal(pData: number): WindowManagerRegisterPointerEventsOnViewParams;
}
declare class WindowManagerRegisterUIElementParams {
    TypeName: string;
    IsFrameworkElement: boolean;
    Classes_Length: number;
    Classes: Array<string>;
    static unmarshal(pData: number): WindowManagerRegisterUIElementParams;
}
declare class WindowManagerRegisterUIElementReturn {
    RegistrationId: number;
    marshal(pData: number): void;
}
declare class WindowManagerRemoveAttributeParams {
    HtmlId: number;
    Name: string;
    static unmarshal(pData: number): WindowManagerRemoveAttributeParams;
}
declare class WindowManagerRemoveViewParams {
    HtmlId: number;
    ChildView: number;
    static unmarshal(pData: number): WindowManagerRemoveViewParams;
}
declare class WindowManagerResetElementBackgroundParams {
    HtmlId: number;
    static unmarshal(pData: number): WindowManagerResetElementBackgroundParams;
}
declare class WindowManagerResetStyleParams {
    HtmlId: number;
    Styles_Length: number;
    Styles: Array<string>;
    static unmarshal(pData: number): WindowManagerResetStyleParams;
}
declare class WindowManagerScrollToOptionsParams {
    Left: number;
    Top: number;
    HasLeft: boolean;
    HasTop: boolean;
    DisableAnimation: boolean;
    HtmlId: number;
    static unmarshal(pData: number): WindowManagerScrollToOptionsParams;
}
declare class WindowManagerSetAttributeParams {
    HtmlId: number;
    Name: string;
    Value: string;
    static unmarshal(pData: number): WindowManagerSetAttributeParams;
}
declare class WindowManagerSetAttributesParams {
    HtmlId: number;
    Pairs_Length: number;
    Pairs: Array<string>;
    static unmarshal(pData: number): WindowManagerSetAttributesParams;
}
declare class WindowManagerSetClassesParams {
    HtmlId: number;
    CssClasses_Length: number;
    CssClasses: Array<string>;
    Index: number;
    static unmarshal(pData: number): WindowManagerSetClassesParams;
}
declare class WindowManagerSetContentHtmlParams {
    HtmlId: number;
    Html: string;
    static unmarshal(pData: number): WindowManagerSetContentHtmlParams;
}
declare class WindowManagerSetElementBackgroundColorParams {
    HtmlId: number;
    Color: number;
    static unmarshal(pData: number): WindowManagerSetElementBackgroundColorParams;
}
declare class WindowManagerSetElementBackgroundGradientParams {
    HtmlId: number;
    CssGradient: string;
    static unmarshal(pData: number): WindowManagerSetElementBackgroundGradientParams;
}
declare class WindowManagerSetElementColorParams {
    HtmlId: number;
    Color: number;
    static unmarshal(pData: number): WindowManagerSetElementColorParams;
}
declare class WindowManagerSetElementFillParams {
    HtmlId: number;
    Color: number;
    static unmarshal(pData: number): WindowManagerSetElementFillParams;
}
declare class WindowManagerSetElementTransformParams {
    HtmlId: number;
    M11: number;
    M12: number;
    M21: number;
    M22: number;
    M31: number;
    M32: number;
    static unmarshal(pData: number): WindowManagerSetElementTransformParams;
}
declare class WindowManagerSetNameParams {
    HtmlId: number;
    Name: string;
    static unmarshal(pData: number): WindowManagerSetNameParams;
}
declare class WindowManagerSetPointerEventsParams {
    HtmlId: number;
    Enabled: boolean;
    static unmarshal(pData: number): WindowManagerSetPointerEventsParams;
}
declare class WindowManagerSetPropertyParams {
    HtmlId: number;
    Pairs_Length: number;
    Pairs: Array<string>;
    static unmarshal(pData: number): WindowManagerSetPropertyParams;
}
declare class WindowManagerSetStyleDoubleParams {
    HtmlId: number;
    Name: string;
    Value: number;
    static unmarshal(pData: number): WindowManagerSetStyleDoubleParams;
}
declare class WindowManagerSetStylesParams {
    HtmlId: number;
    Pairs_Length: number;
    Pairs: Array<string>;
    static unmarshal(pData: number): WindowManagerSetStylesParams;
}
declare class WindowManagerSetSvgElementRectParams {
    X: number;
    Y: number;
    Width: number;
    Height: number;
    HtmlId: number;
    static unmarshal(pData: number): WindowManagerSetSvgElementRectParams;
}
declare class WindowManagerSetUnsetClassesParams {
    HtmlId: number;
    CssClassesToSet_Length: number;
    CssClassesToSet: Array<string>;
    CssClassesToUnset_Length: number;
    CssClassesToUnset: Array<string>;
    static unmarshal(pData: number): WindowManagerSetUnsetClassesParams;
}
declare class WindowManagerSetVisibilityParams {
    HtmlId: number;
    Visible: boolean;
    static unmarshal(pData: number): WindowManagerSetVisibilityParams;
}
declare class WindowManagerSetXUidParams {
    HtmlId: number;
    Uid: string;
    static unmarshal(pData: number): WindowManagerSetXUidParams;
}
declare namespace Windows.ApplicationModel.DataTransfer.DragDrop.Core {
    class DragDropExtensionEventArgs {
        eventName: string;
        allowedOperations: string;
        acceptedOperation: string;
        dataItems: string;
        timestamp: number;
        x: number;
        y: number;
        id: number;
        buttons: number;
        shift: boolean;
        ctrl: boolean;
        alt: boolean;
        static unmarshal(pData: number): DragDropExtensionEventArgs;
        marshal(pData: number): void;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_ClearParams {
        Locality: string;
        static unmarshal(pData: number): ApplicationDataContainer_ClearParams;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_ContainsKeyParams {
        Key: string;
        Value: string;
        Locality: string;
        static unmarshal(pData: number): ApplicationDataContainer_ContainsKeyParams;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_ContainsKeyReturn {
        ContainsKey: boolean;
        marshal(pData: number): void;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_GetCountParams {
        Locality: string;
        static unmarshal(pData: number): ApplicationDataContainer_GetCountParams;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_GetCountReturn {
        Count: number;
        marshal(pData: number): void;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_GetKeyByIndexParams {
        Locality: string;
        Index: number;
        static unmarshal(pData: number): ApplicationDataContainer_GetKeyByIndexParams;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_GetKeyByIndexReturn {
        Value: string;
        marshal(pData: number): void;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_GetValueByIndexParams {
        Locality: string;
        Index: number;
        static unmarshal(pData: number): ApplicationDataContainer_GetValueByIndexParams;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_GetValueByIndexReturn {
        Value: string;
        marshal(pData: number): void;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_RemoveParams {
        Locality: string;
        Key: string;
        static unmarshal(pData: number): ApplicationDataContainer_RemoveParams;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_RemoveReturn {
        Removed: boolean;
        marshal(pData: number): void;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_SetValueParams {
        Key: string;
        Value: string;
        Locality: string;
        static unmarshal(pData: number): ApplicationDataContainer_SetValueParams;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_TryGetValueParams {
        Key: string;
        Locality: string;
        static unmarshal(pData: number): ApplicationDataContainer_TryGetValueParams;
    }
}
declare namespace Windows.Storage {
    class ApplicationDataContainer_TryGetValueReturn {
        Value: string;
        HasValue: boolean;
        marshal(pData: number): void;
    }
}
declare namespace Windows.Storage {
    class StorageFolderMakePersistentParams {
        Paths_Length: number;
        Paths: Array<string>;
        static unmarshal(pData: number): StorageFolderMakePersistentParams;
    }
}
declare namespace Windows.UI.Xaml {
    class NativePointerEventArgs {
        HtmlId: number;
        Event: number;
        pointerId: number;
        x: number;
        y: number;
        ctrl: boolean;
        shift: boolean;
        buttons: number;
        buttonUpdate: number;
        deviceType: number;
        srcHandle: number;
        timestamp: number;
        pressure: number;
        wheelDeltaX: number;
        wheelDeltaY: number;
        hasRelatedTarget: boolean;
        marshal(pData: number): void;
    }
}
declare namespace Windows.UI.Xaml {
    class NativePointerEventResult {
        Result: number;
        static unmarshal(pData: number): NativePointerEventResult;
    }
}
declare namespace Windows.UI.Xaml {
    class NativePointerSubscriptionParams {
        HtmlId: number;
        Events: number;
        static unmarshal(pData: number): NativePointerSubscriptionParams;
    }
}
