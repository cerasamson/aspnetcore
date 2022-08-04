// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

export async function setImage(data: any, imageId: string) {
    const arrayBuffer = await data.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const image = document.getElementById(imageId) as HTMLImageElement | null;
    if (image != null) {
        image.src = url;
    }
    URL.revokeObjectURL(url);
}
